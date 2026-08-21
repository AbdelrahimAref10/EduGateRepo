using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.EndLessonGroupSession;

public sealed class EndLessonGroupSessionCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<EndLessonGroupSessionCommand, Result<LessonGroupSessionDto>>
{
    public async Task<Result<LessonGroupSessionDto>> Handle(
        EndLessonGroupSessionCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonGroupSessionDto>.NotFound("Teacher profile was not found.");

        var session = await dbContext.LessonGroupSessions
            .AsTracking()
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.SessionId
                     && x.LessonGroupId == request.GroupId
                     && x.LessonGroup.LessonId == request.LessonId
                     && x.LessonGroup.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (session is null)
            return Result<LessonGroupSessionDto>.NotFound("الحصة غير موجودة.");

        if (!session.StartedAtUtc.HasValue)
            return Result<LessonGroupSessionDto>.Failure("ابدأ الحصة أولاً قبل إنهائها.");

        if (!session.EndedAtUtc.HasValue)
        {
            session.EndedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<LessonGroupSessionDto>.Success(
            LessonMappings.ToSessionDto(session, session.LessonGroup.EndedAtUtc.HasValue));
    }
}
