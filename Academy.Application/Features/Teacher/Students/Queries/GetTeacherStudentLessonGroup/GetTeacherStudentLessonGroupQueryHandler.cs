using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Students.Common;
using Academy.Application.Features.Teacher.Students.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Students.Queries.GetTeacherStudentLessonGroup;

public sealed class GetTeacherStudentLessonGroupQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetTeacherStudentLessonGroupQuery, Result<TeacherStudentGroupDto>>
{
    public async Task<Result<TeacherStudentGroupDto>> Handle(
        GetTeacherStudentLessonGroupQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherStudentAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<TeacherStudentGroupDto>.NotFound("Teacher profile was not found.");

        var isStudent = await TeacherStudentAccess.IsTeachersConfirmedStudentAsync(
            dbContext, teacherId.Value, request.StudentId, cancellationToken);

        if (!isStudent)
            return Result<TeacherStudentGroupDto>.NotFound("Student was not found.");

        var ownsLesson = await TeacherStudentAccess.OwnsLessonAsync(
            dbContext, teacherId.Value, request.LessonId, cancellationToken);

        if (!ownsLesson)
            return Result<TeacherStudentGroupDto>.NotFound("Lesson was not found.");

        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var group = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x =>
                x.LessonId == request.LessonId
                && x.Lesson.TeacherId == teacherId.Value
                && x.Members.Any(m => m.StudentId == request.StudentId))
            .ToDto(request.StudentId, isArabic)
            .FirstOrDefaultAsync(cancellationToken);

        return group is null
            ? Result<TeacherStudentGroupDto>.NotFound("الطالب غير مشترك في مجموعة لهذا الدرس.")
            : Result<TeacherStudentGroupDto>.Success(group);
    }
}
