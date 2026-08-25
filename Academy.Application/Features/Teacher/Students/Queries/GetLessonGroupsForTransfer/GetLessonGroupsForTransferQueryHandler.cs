using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Students.Common;
using Academy.Application.Features.Teacher.Students.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Students.Queries.GetLessonGroupsForTransfer;

public sealed class GetLessonGroupsForTransferQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonGroupsForTransferQuery, Result<IReadOnlyList<TeacherStudentGroupDto>>>
{
    public async Task<Result<IReadOnlyList<TeacherStudentGroupDto>>> Handle(
        GetLessonGroupsForTransferQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherStudentAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<TeacherStudentGroupDto>>.NotFound("Teacher profile was not found.");

        var isStudent = await TeacherStudentAccess.IsTeachersConfirmedStudentAsync(
            dbContext, teacherId.Value, request.StudentId, cancellationToken);

        if (!isStudent)
            return Result<IReadOnlyList<TeacherStudentGroupDto>>.NotFound("Student was not found.");

        var ownsLesson = await TeacherStudentAccess.OwnsLessonAsync(
            dbContext, teacherId.Value, request.LessonId, cancellationToken);

        if (!ownsLesson)
            return Result<IReadOnlyList<TeacherStudentGroupDto>>.NotFound("Lesson was not found.");

        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var groups = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId && x.Lesson.TeacherId == teacherId.Value)
            .OrderBy(x => x.CreatedAtUtc)
            .ToDto(request.StudentId, isArabic)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TeacherStudentGroupDto>>.Success(groups);
    }
}
