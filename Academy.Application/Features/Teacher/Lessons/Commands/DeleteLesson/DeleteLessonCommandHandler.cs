using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.DeleteLesson;

public sealed class DeleteLessonCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteLessonCommand, Result>
{
    public async Task<Result> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result.NotFound("Teacher profile was not found.");

        var lesson = await dbContext.Lessons
            .AsTracking()
            .Include(x => x.Groups)
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (lesson is null)
            return Result.NotFound("Lesson was not found.");

        if (lesson.Groups.Any(g => g.StartedAtUtc.HasValue))
            return Result.Conflict("لا يمكن حذف الدرس بعد بدء أول مجموعة.");

        var bookings = await dbContext.LessonBookings
            .AsTracking()
            .Where(x => x.LessonId == lesson.Id)
            .ToListAsync(cancellationToken);

        var groupIds = lesson.Groups.Select(g => g.Id).ToList();
        if (groupIds.Count > 0)
        {
            var members = await dbContext.LessonGroupMembers
                .AsTracking()
                .Where(x => groupIds.Contains(x.LessonGroupId))
                .ToListAsync(cancellationToken);

            var dates = await dbContext.LessonGroupDates
                .AsTracking()
                .Where(x => groupIds.Contains(x.LessonGroupId))
                .ToListAsync(cancellationToken);

            var sessions = await dbContext.LessonGroupSessions
                .AsTracking()
                .Where(x => groupIds.Contains(x.LessonGroupId))
                .ToListAsync(cancellationToken);

            dbContext.LessonGroupMembers.RemoveRange(members);
            dbContext.LessonGroupDates.RemoveRange(dates);
            dbContext.LessonGroupSessions.RemoveRange(sessions);
            dbContext.LessonGroups.RemoveRange(lesson.Groups);
        }

        dbContext.LessonBookings.RemoveRange(bookings);
        dbContext.Lessons.Remove(lesson);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
