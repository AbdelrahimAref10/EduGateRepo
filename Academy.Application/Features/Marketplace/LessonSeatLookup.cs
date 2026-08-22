using Academy.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace;

public static class LessonSeatLookup
{
    public static async Task<IReadOnlyDictionary<int, LessonSeatAvailability>> ForLessonsAsync(
        IApplicationDbContext dbContext,
        IEnumerable<int> lessonIds,
        CancellationToken cancellationToken)
    {
        var ids = lessonIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, LessonSeatAvailability>();

        var rows = await dbContext.LessonGroups
            .Where(g => ids.Contains(g.LessonId))
            .Select(g => new
            {
                g.LessonId,
                g.MaxCapacity,
                g.EndedAtUtc,
                MembersCount = g.Members.Count
            })
            .ToListAsync(cancellationToken);

        return ids.ToDictionary(
            id => id,
            id => LessonSeatCalculator.FromGroups(
                rows.Where(r => r.LessonId == id)
                    .Select(r => new LessonGroupSeatInput(r.MaxCapacity, r.MembersCount, r.EndedAtUtc))));
    }
}
