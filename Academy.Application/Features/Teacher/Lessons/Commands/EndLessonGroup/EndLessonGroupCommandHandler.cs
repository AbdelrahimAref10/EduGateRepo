using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.EndLessonGroup;

public sealed class EndLessonGroupCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<EndLessonGroupCommand, Result<LessonGroupDto>>
{
    public async Task<Result<LessonGroupDto>> Handle(
        EndLessonGroupCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonGroupDto>.NotFound("Teacher profile was not found.");

        var group = await dbContext.LessonGroups
            .AsTracking()
            .Include(x => x.Lesson)
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
            .Include(x => x.Dates)
            .Include(x => x.Sessions)
            .Include(x => x.Members)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (group is null)
            return Result<LessonGroupDto>.NotFound("Group was not found.");

        if (!group.StartedAtUtc.HasValue)
            return Result<LessonGroupDto>.Failure("لا يمكن إنهاء مجموعة لم تبدأ بعد.");

        if (!group.EndedAtUtc.HasValue)
        {
            var now = DateTime.UtcNow;
            group.EndedAtUtc = now;

            foreach (var session in group.Sessions.Where(s => s.StartedAtUtc.HasValue && !s.EndedAtUtc.HasValue))
                session.EndedAtUtc = now;

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<LessonGroupDto>.Success(
            LessonMappings.ToGroupDto(group, requestLanguage.Current, includeSessions: true));
    }
}
