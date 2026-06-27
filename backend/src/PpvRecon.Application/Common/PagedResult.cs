namespace PpvRecon.Application.Common;

public sealed class PagedResult<T>
{
    public const int FixedPageSize = 100;

    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; } = FixedPageSize;
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}
