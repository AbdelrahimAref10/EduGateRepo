namespace Academy.Application.Common.Models;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required int TotalCount { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public int TotalPages =>
        PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Empty(int page, int pageSize) =>
        new()
        {
            Items = [],
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize) =>
        new()
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
}

public static class Paging
{
    public const int DefaultPageSize = 9;

    public const int MaxPageSize = 48;

    public static (int Page, int PageSize, int Skip) Normalize(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? 1 : page.Value;
        var size = pageSize is null or < 1
            ? DefaultPageSize
            : Math.Clamp(pageSize.Value, 1, MaxPageSize);
        return (p, size, (p - 1) * size);
    }
}
