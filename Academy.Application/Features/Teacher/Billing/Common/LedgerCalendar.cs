namespace Academy.Application.Features.Teacher.Billing.Common;

internal static class LedgerCalendar
{
    private static readonly TimeZoneInfo Egypt = ResolveEgypt();

    public static (DateTime FromUtc, DateTime ToUtcExclusive) TodayWindow(DateTime utcNow)
    {
        var utc = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, Egypt);
        return DayWindow(DateOnly.FromDateTime(local));
    }

    public static (DateTime FromUtc, DateTime ToUtcExclusive) DayWindow(DateOnly date)
    {
        var startLocal = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var endLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return (
            TimeZoneInfo.ConvertTimeToUtc(startLocal, Egypt),
            TimeZoneInfo.ConvertTimeToUtc(endLocal, Egypt));
    }

    public static (DateTime? FromUtc, DateTime? ToUtcExclusive) Range(DateOnly? from, DateOnly? to)
    {
        DateTime? fromUtc = from is DateOnly f ? DayWindow(f).FromUtc : null;
        DateTime? toUtc = to is DateOnly t ? DayWindow(t).ToUtcExclusive : null;
        return (fromUtc, toUtc);
    }

    private static TimeZoneInfo ResolveEgypt()
    {
        foreach (var id in new[] { "Africa/Cairo", "Egypt Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            "Egypt",
            TimeSpan.FromHours(3),
            "Egypt",
            "Egypt");
    }
}
