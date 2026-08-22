using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonManage;

public sealed class GetLessonManageQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonManageQuery, Result<LessonManageDto>>
{
    public async Task<Result<LessonManageDto>> Handle(
        GetLessonManageQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await LessonReadQueries.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<LessonManageDto>.NotFound("Teacher profile was not found.");

        var language = requestLanguage.Current;
        var lesson = await LessonReadQueries.GetLessonHeaderAsync(
            dbContext, teacherId.Value, request.LessonId, language, cancellationToken);

        if (lesson is null)
            return Result<LessonManageDto>.NotFound("Lesson was not found.");

        var students = await LessonReadQueries.GetLessonStudentsAsync(
            dbContext, request.LessonId, confirmedOnly: false, cancellationToken);

        var groups = await LessonReadQueries.GetLessonGroupsAsync(
            dbContext, request.LessonId, language, cancellationToken);

        return Result<LessonManageDto>.Success(new LessonManageDto
        {
            Lesson = lesson,
            Students = students,
            Groups = groups
        });
    }
}
