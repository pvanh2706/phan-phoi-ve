using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpvRecon.Application.Common;
using PpvRecon.Application.Summaries;
using PpvRecon.Domain.Enums;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bank-transaction-details")]
public sealed class BankTransactionDetailsController(PpvReconDbContext dbContext) : PpvControllerBase
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
}
