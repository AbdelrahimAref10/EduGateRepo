using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.RemoveGroupMember;

public sealed class RemoveGroupMemberCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RemoveGroupMemberCommand, Result>
{
    public async Task<Result> Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result.NotFound("Teacher profile was not found.");

        var group = await dbContext.LessonGroups
            .Include(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (group is null)
            return Result.NotFound("Group was not found.");

        if (group.StartedAtUtc.HasValue)
            return Result.Conflict("لا يمكن إزالة طلاب بعد بدء المجموعة.");

        var member = await dbContext.LessonGroupMembers
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.LessonGroupId == group.Id && x.StudentId == request.StudentId,
                cancellationToken);

        if (member is null)
            return Result.NotFound("الطالب غير موجود في هذه المجموعة.");

        dbContext.LessonGroupMembers.Remove(member);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
