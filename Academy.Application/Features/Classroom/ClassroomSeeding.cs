using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Classroom;

internal static class ClassroomSeeding
{
    /// <summary>
    /// Ensures a roster row exists for every current group member.
    /// </summary>
    public static async Task EnsureStudentDetailsAsync(
        IApplicationDbContext dbContext,
        LessonGroupSession session,
        CancellationToken cancellationToken)
    {
        var memberIds = await dbContext.LessonGroupMembers
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .Select(x => x.StudentId)
            .ToListAsync(cancellationToken);

        if (memberIds.Count == 0)
            return;

        var existing = await dbContext.LessonSessionStudentDetails
            .Where(x => x.LessonGroupSessionId == session.Id)
            .Select(x => x.StudentId)
            .ToListAsync(cancellationToken);

        var existingSet = existing.ToHashSet();
        var now = DateTime.UtcNow;

        foreach (var studentId in memberIds.Where(id => !existingSet.Contains(id)))
        {
            dbContext.LessonSessionStudentDetails.Add(new LessonSessionStudentDetail
            {
                LessonGroupSessionId = session.Id,
                StudentId = studentId,
                IsPresent = false,
                CreatedAtUtc = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Seeds roster only for explicitly invited students (makeup sessions).
    /// </summary>
    public static async Task EnsureInvitedStudentDetailsAsync(
        IApplicationDbContext dbContext,
        LessonGroupSession session,
        IReadOnlyList<int> studentIds,
        CancellationToken cancellationToken)
    {
        if (studentIds.Count == 0)
            return;

        var existing = await dbContext.LessonSessionStudentDetails
            .Where(x => x.LessonGroupSessionId == session.Id)
            .Select(x => x.StudentId)
            .ToListAsync(cancellationToken);

        var existingSet = existing.ToHashSet();
        var now = DateTime.UtcNow;

        foreach (var studentId in studentIds.Where(id => !existingSet.Contains(id)))
        {
            dbContext.LessonSessionStudentDetails.Add(new LessonSessionStudentDetail
            {
                LessonGroupSessionId = session.Id,
                StudentId = studentId,
                IsPresent = false,
                CreatedAtUtc = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
