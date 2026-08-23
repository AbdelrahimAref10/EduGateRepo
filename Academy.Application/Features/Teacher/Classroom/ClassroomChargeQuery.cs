using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom;

/// <summary>
/// Loads charge rows for classroom billing display (read-side). Status math lives on <see cref="Charge"/>.
/// </summary>
internal static class ClassroomChargeQuery
{
    public static async Task<List<Charge>> ForStudentAsync(
        IApplicationDbContext dbContext,
        Lesson lesson,
        LessonGroupSession session,
        int studentId,
        CancellationToken cancellationToken)
    {
        if (lesson.IsPerSession || session.IsMakeup)
        {
            var sessionCharges = await dbContext.Charges
                .AsNoTracking()
                .Include(x => x.Allocations)
                .Where(x =>
                    x.LessonGroupSessionId == session.Id
                    && x.StudentId == studentId
                    && (x.Type == ChargeType.Session || x.Type == ChargeType.Makeup)
                    && x.Status != ChargeStatus.Deferred)
                .ToListAsync(cancellationToken);
            return SyncAllocatedFromAllocations(sessionCharges);
        }

        var cycles = await dbContext.Charges
            .AsNoTracking()
            .Include(x => x.Allocations)
            .Where(x =>
                x.LessonId == lesson.Id
                && x.StudentId == studentId
                && x.Type == ChargeType.MonthlyCycle)
            .ToListAsync(cancellationToken);

        var cycle = cycles.FirstOrDefault(c => c.CoversDate(session.SessionDate));
        if (cycle is null)
        {
            var open = await dbContext.Charges
                .AsNoTracking()
                .Include(x => x.Allocations)
                .Where(x =>
                    x.LessonId == lesson.Id
                    && x.StudentId == studentId
                    && x.Status != ChargeStatus.Deferred
                    && x.Status != ChargeStatus.Paid)
                .ToListAsync(cancellationToken);
            return SyncAllocatedFromAllocations(open);
        }

        var children = await dbContext.Charges
            .AsNoTracking()
            .Include(x => x.Allocations)
            .Where(x => x.ParentChargeId == cycle.Id && x.Status != ChargeStatus.Deferred)
            .ToListAsync(cancellationToken);

        return SyncAllocatedFromAllocations([cycle, .. children]);
    }

    private static List<Charge> SyncAllocatedFromAllocations(List<Charge> charges)
    {
        foreach (var charge in charges)
        {
            var allocated = charge.Allocations?.Sum(a => a.Amount) ?? 0m;
            charge.AllocatedAmount = allocated;
            charge.RecalculateStatus();
        }

        return charges;
    }
}
