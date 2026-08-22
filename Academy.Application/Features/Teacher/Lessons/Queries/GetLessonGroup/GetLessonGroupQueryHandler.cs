using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroup;

public sealed class GetLessonGroupQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonGroupQuery, Result<LessonGroupManageDto>>
{
    public async Task<Result<LessonGroupManageDto>> Handle(
        GetLessonGroupQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await LessonReadQueries.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<LessonGroupManageDto>.NotFound("Teacher profile was not found.");

        var group = await LessonReadQueries.GetGroupAsync(
            dbContext,
            teacherId.Value,
            request.LessonId,
            request.GroupId,
            requestLanguage.Current,
            cancellationToken);

        if (group is null)
            return Result<LessonGroupManageDto>.NotFound("Group was not found.");

        var students = await LessonReadQueries.GetLessonStudentsAsync(
            dbContext, request.LessonId, confirmedOnly: true, cancellationToken);

        return Result<LessonGroupManageDto>.Success(new LessonGroupManageDto
        {
            Group = group,
            Students = students
        });
    }
}
