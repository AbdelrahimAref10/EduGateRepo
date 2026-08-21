using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroup;

public sealed class GetLessonGroupQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonGroupQuery, Result<LessonGroupDto>>
{
    public async Task<Result<LessonGroupDto>> Handle(
        GetLessonGroupQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonGroupDto>.NotFound("Teacher profile was not found.");

        var group = await dbContext.LessonGroups
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

        return Result<LessonGroupDto>.Success(
            LessonMappings.ToGroupDto(group, requestLanguage.Current, includeSessions: true));
    }
}
