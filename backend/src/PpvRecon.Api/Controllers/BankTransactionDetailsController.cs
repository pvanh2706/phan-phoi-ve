using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services.BankStatement;
using PpvRecon.Application.Auditing;
using PpvRecon.Application.Common;
using PpvRecon.Application.Jobs;
using PpvRecon.Application.Reconciliation;
using PpvRecon.Application.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bank-transaction-details")]
public sealed class BankTransactionDetailsController(
    PpvReconDbContext dbContext,
    IJobRunner jobRunner,
    IBankStatementSyncService bankStatementSyncService,
    IReconciliationBuilder reconciliationBuilder,
    IAuditService auditService) : PpvControllerBase
{
    private const long MaxUploadBytes = 20 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonLineItemsOptions = new() { PropertyNameCaseInsensitive = true };

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BankTransactionDetailDto>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] ParkPaymentType? paymentType = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        var query = dbContext.BankTransactionDetails.AsNoTracking();

        if (dateFrom is not null) query = query.Where(x => x.BusinessDate >= dateFrom);
        if (dateTo is not null) query = query.Where(x => x.BusinessDate <= dateTo);
        if (paymentType is not null) query = query.Where(x => x.PaymentType == paymentType);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.Content, $"%{kw}%")
                || (x.BankAccount != null && EF.Functions.Like(x.BankAccount, $"%{kw}%")));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.BusinessDate)
            .ThenByDescending(x => x.TransactionAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * PagedResult<BankTransactionDetailDto>.FixedPageSize)
            .Take(PagedResult<BankTransactionDetailDto>.FixedPageSize)
            .Select(x => new
            {
                Dto = new BankTransactionDetailDto
                {
                    Id = x.Id,
                    BusinessDate = x.BusinessDate,
                    TransactionAtUtc = x.TransactionAtUtc,
                    PaymentType = x.PaymentType,
                    DebitAmount = x.DebitAmount,
                    CreditAmount = x.CreditAmount,
                    Content = x.Content,
                    BankAccount = x.BankAccount,
                    ParkId = x.ParkId,
                    ParkName = x.ParkId == null
                        ? null
                        : dbContext.Parks.Where(p => p.Id == x.ParkId).Select(p => p.Name).FirstOrDefault(),
                    SourceType = x.SourceType,
                    CreatedAtUtc = x.CreatedAtUtc,
                    UpdatedAtUtc = x.UpdatedAtUtc,
                },
                x.LineItemsJson,
            })
            .ToListAsync(cancellationToken);

        // Chi tiết từng giao dịch (khi gộp) được giải mã ngoài bộ nhớ vì EF không dịch được JSON.
        var items = rows.Select(r =>
        {
            if (!string.IsNullOrEmpty(r.LineItemsJson))
                r.Dto.LineItems = JsonSerializer.Deserialize<List<BankTransactionLineItemDto>>(r.LineItemsJson, JsonLineItemsOptions);
            return r.Dto;
        }).ToList();

        return Ok(ApiResponse<PagedResult<BankTransactionDetailDto>>.Ok(new PagedResult<BankTransactionDetailDto>
        {
            Items = items,
            Page = page,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PagedResult<BankTransactionDetailDto>.FixedPageSize),
        }));
    }

    /// <summary>
    /// Lấy sao kê BIDV từ email → trích PDF → parse → map Park qua TKThe →
    /// gộp theo (KVC, ngày) → ghi đè theo ngày vào bảng BankTransactionDetails. (Nút "Get API")
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<ApiResponse<BankStatementSyncResult>>> Sync(
        CancellationToken cancellationToken = default)
    {
        // Đi qua JobRunner để lần chạy thủ công cũng được ghi log đầy đủ
        // (JobRun + log gọi API + audit), giống lần chạy tự động theo lịch.
        var detail = await jobRunner.RunExternalSyncAsync(
            ExternalApiSource.BankTransaction,
            VietnamToday(),
            JobTriggerType.Manual,
            CurrentUserId,
            cancellationToken);

        if (detail.Status is JobRunStatus.Failed or JobRunStatus.CompletedWithErrors)
        {
            return BadRequest(ApiResponse<BankStatementSyncResult>.Fail(
                $"Không lấy được sao kê từ email: {detail.ErrorMessage}"));
        }

        var result = ExtractSyncResult(detail.SummaryJson) ?? new BankStatementSyncResult();

        var message = result.Imported == 0
            ? "Không có giao dịch nào được nhập."
            : $"Đã nhập {result.Imported} dòng KVC (gộp từ {result.TransactionsParsed} giao dịch) từ {result.MailsProcessed} email.";
        if (result.SkippedUnmatched > 0)
            message += $" Bỏ qua {result.SkippedUnmatched} giao dịch không khớp KVC.";

        return Ok(ApiResponse<BankStatementSyncResult>.Ok(result, message));
    }

    /// <summary>
    /// Nhập tay sao kê từ 1 file PDF tải lên, dùng khi ngân hàng lỗi/không gửi sao kê về email.
    /// Xử lý qua đúng luồng map Park + gộp theo ngày + ghi đè như "Get API".
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<ApiResponse<BankStatementSyncResult>>> Upload(
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<BankStatementSyncResult>.Fail("Vui lòng chọn file PDF sao kê ngân hàng."));
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<BankStatementSyncResult>.Fail("Chỉ chấp nhận file PDF sao kê."));
        }

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        BankStatementSyncResult result;
        try
        {
            result = await bankStatementSyncService.ImportFromPdfAsync(stream.ToArray(), CurrentUserId, cancellationToken);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BankStatementSyncResult>.Fail($"Không xử lý được file sao kê: {ex.Message}"));
        }

        await auditService.LogAsync(new AuditLogEntry
        {
            UserId = CurrentUserId,
            Module = "BankStatement",
            EntityName = "BankTransactionDetail",
            Action = AuditAction.ManualEntry,
            After = new
            {
                fileName = file.FileName,
                result.TransactionsParsed,
                result.Imported,
                result.SkippedUnmatched,
                result.OverwrittenBusinessDates,
            },
        }, cancellationToken);

        foreach (var date in result.OverwrittenBusinessDates)
        {
            await reconciliationBuilder.BuildAsync(date, CurrentUserId, cancellationToken, JobTriggerType.Manual);
        }

        var message = result.Imported == 0
            ? "Không có giao dịch nào được nhập từ file."
            : $"Đã nhập {result.Imported} dòng KVC (gộp từ {result.TransactionsParsed} giao dịch) từ file tải lên.";
        if (result.SkippedUnmatched > 0)
            message += $" Bỏ qua {result.SkippedUnmatched} giao dịch không khớp KVC.";

        return Ok(ApiResponse<BankStatementSyncResult>.Ok(result, message));
    }

    /// <summary>Đọc lại BankStatementSyncResult từ trường "result" trong SummaryJson của lần chạy job.</summary>
    private static BankStatementSyncResult? ExtractSyncResult(string? summaryJson)
    {
        if (string.IsNullOrWhiteSpace(summaryJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(summaryJson);
            if (doc.RootElement.TryGetProperty("result", out var resultElement)
                && resultElement.ValueKind == JsonValueKind.Object)
            {
                return resultElement.Deserialize<BankStatementSyncResult>(JsonLineItemsOptions);
            }
        }
        catch (JsonException)
        {
            // SummaryJson sai định dạng -> coi như không có chi tiết.
        }
        return null;
    }

    private static DateOnly VietnamToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        }
        catch (TimeZoneNotFoundException)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        }
    }
}
