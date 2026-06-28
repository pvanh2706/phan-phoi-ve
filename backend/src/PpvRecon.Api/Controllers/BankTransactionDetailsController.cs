using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Api.Services.BankStatement;
using PpvRecon.Application.Common;
using PpvRecon.Application.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bank-transaction-details")]
public sealed class BankTransactionDetailsController(
    PpvReconDbContext dbContext,
    IBankStatementSyncService syncService) : PpvControllerBase
{
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
        var items = await query
            .OrderByDescending(x => x.BusinessDate)
            .ThenByDescending(x => x.TransactionAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * PagedResult<BankTransactionDetailDto>.FixedPageSize)
            .Take(PagedResult<BankTransactionDetailDto>.FixedPageSize)
            .Select(x => new BankTransactionDetailDto
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
            })
            .ToListAsync(cancellationToken);

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
    /// ghi đè theo ngày vào bảng BankTransactionDetails. (Nút "Get API")
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<ApiResponse<BankStatementSyncResult>>> Sync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await syncService.SyncAsync(CurrentUserId, cancellationToken);

            var message = result.Imported == 0
                ? "Không có giao dịch nào được nhập."
                : $"Đã nhập {result.Imported} giao dịch từ {result.MailsProcessed} email.";
            if (result.SkippedUnmatched > 0)
                message += $" Bỏ qua {result.SkippedUnmatched} giao dịch không khớp KVC.";

            return Ok(ApiResponse<BankStatementSyncResult>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BankStatementSyncResult>.Fail(
                $"Không lấy được sao kê từ email: {ex.Message}"));
        }
    }
}
