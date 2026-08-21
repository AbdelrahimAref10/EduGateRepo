using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.DeleteLessonGroup;

public sealed class DeleteLessonGroupCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteLessonGroupCommand, Result>
{
    public async Task<Result> Handle(DeleteLessonGroupCommand request, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result.NotFound("Teacher profile was not found.");

        var group = await dbContext.LessonGroups
            .AsTracking()
            .Include(x => x.Lesson)
            .Include(x => x.Members)
            .Include(x => x.Dates)
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (group is null)
            return Result.NotFound("Group was not found.");

        var canDelete = !group.StartedAtUtc.HasValue || group.EndedAtUtc.HasValue;
        if (!canDelete)
            return Result.Conflict("لا يمكن حذف المجموعة بعد بدايتها إلا بعد إنهائها.");

        dbContext.LessonGroupMembers.RemoveRange(group.Members);
        dbContext.LessonGroupDates.RemoveRange(group.Dates);
        dbContext.LessonGroupSessions.RemoveRange(group.Sessions);
        dbContext.LessonGroups.Remove(group);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
