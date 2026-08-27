namespace Academy.Application.Features.Teacher.Billing.Common;

public static class LedgerPaging
{
    public const int PageSize = 10;

    public static (int Page, int PageSize, int Skip) Normalize(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? 1 : page.Value;
        var size = pageSize is null or < 1
            ? PageSize
            : Math.Clamp(pageSize.Value, 1, PageSize);
        return (p, size, (p - 1) * size);
    }
}
