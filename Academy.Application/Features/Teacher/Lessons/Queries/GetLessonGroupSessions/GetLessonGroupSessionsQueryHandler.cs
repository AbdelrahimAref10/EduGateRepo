using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroupSessions;

public sealed class GetLessonGroupSessionsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLessonGroupSessionsQuery, Result<IReadOnlyList<LessonGroupSessionDto>>>
{
    public async Task<Result<IReadOnlyList<LessonGroupSessionDto>>> Handle(
        GetLessonGroupSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await LessonReadQueries.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LessonGroupSessionDto>>.NotFound("Teacher profile was not found.");

        var exists = await LessonReadQueries.GroupExistsAsync(
            dbContext, teacherId.Value, request.LessonId, request.GroupId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<LessonGroupSessionDto>>.NotFound("Group was not found.");

        var sessions = await LessonReadQueries.GetGroupSessionsAsync(
            dbContext, request.GroupId, cancellationToken);

        return Result<IReadOnlyList<LessonGroupSessionDto>>.Success(sessions);
    }
}
