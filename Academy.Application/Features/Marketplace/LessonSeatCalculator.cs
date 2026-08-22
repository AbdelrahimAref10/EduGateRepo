namespace Academy.Application.Features.Marketplace;

public readonly record struct LessonGroupSeatInput(
    int? MaxCapacity,
    int MembersCount,
    DateTime? EndedAtUtc);

public readonly record struct LessonSeatAvailability(int? RemainingSeats, bool SeatsOpen, bool IsFull)
{
    public static LessonSeatAvailability Open() => new(null, true, false);
}

public static class LessonSeatCalculator
{
    public static LessonSeatAvailability FromGroups(IEnumerable<LessonGroupSeatInput> groups)
    {
        var active = groups.Where(g => g.EndedAtUtc is null).ToList();
        if (active.Count == 0 || active.Any(g => g.MaxCapacity is null))
            return LessonSeatAvailability.Open();

        var remaining = active.Sum(g => Math.Max(0, g.MaxCapacity!.Value - g.MembersCount));
        return new LessonSeatAvailability(remaining, false, remaining == 0);
    }
}
