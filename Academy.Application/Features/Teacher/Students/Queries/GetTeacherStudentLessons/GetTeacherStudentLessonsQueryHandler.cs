using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Students.Common;
using Academy.Application.Features.Teacher.Students.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Students.Queries.GetTeacherStudentLessons;

public sealed class GetTeacherStudentLessonsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetTeacherStudentLessonsQuery, Result<IReadOnlyList<TeacherStudentLessonDto>>>
{
    public async Task<Result<IReadOnlyList<TeacherStudentLessonDto>>> Handle(
        GetTeacherStudentLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherStudentAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<TeacherStudentLessonDto>>.NotFound("Teacher profile was not found.");

        var isStudent = await TeacherStudentAccess.IsTeachersConfirmedStudentAsync(
            dbContext, teacherId.Value, request.StudentId, cancellationToken);

        if (!isStudent)
            return Result<IReadOnlyList<TeacherStudentLessonDto>>.NotFound("Student was not found.");

        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var lessons = await dbContext.LessonBookings
            .AsNoTracking()
            .Where(x =>
                x.TeacherId == teacherId.Value
                && x.StudentId == request.StudentId
                && x.Status == BookingStatus.Confirmed)
            .OrderByDescending(x => x.Lesson.CreatedAtUtc)
            .Select(x => new TeacherStudentLessonDto
            {
                LessonId = x.LessonId,
                Subject = isArabic ? x.Lesson.EducationSubject.NameAr : x.Lesson.EducationSubject.NameEn,
                AcademicYearName = x.Lesson.AcademicYear.Name,
                EducationStageName = isArabic ? x.Lesson.EducationStage.NameAr : x.Lesson.EducationStage.NameEn,
                EducationYearName = isArabic ? x.Lesson.EducationYear.NameAr : x.Lesson.EducationYear.NameEn,
                BillingType = x.Lesson.BillingType.ToString(),
                StartDate = x.Lesson.StartDate,
                IsActive = x.Lesson.IsActive
            })
            .ToListAsync(cancellationToken);

        if (lessons.Count == 0)
            return Result<IReadOnlyList<TeacherStudentLessonDto>>.Success(lessons);

        var lessonIds = lessons.Select(x => x.LessonId).ToList();

        var memberships = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(m => m.StudentId == request.StudentId && lessonIds.Contains(m.LessonGroup.LessonId))
            .Select(m => new
            {
                m.LessonGroup.LessonId,
                m.LessonGroupId,
                m.LessonGroup.Name
            })
            .ToListAsync(cancellationToken);

        var byLesson = memberships
            .GroupBy(x => x.LessonId)
            .ToDictionary(g => g.Key, g => g.First());

        IReadOnlyList<TeacherStudentLessonDto> items = lessons
            .Select(x =>
            {
                byLesson.TryGetValue(x.LessonId, out var membership);
                return new TeacherStudentLessonDto
                {
                    LessonId = x.LessonId,
                    Subject = x.Subject,
                    AcademicYearName = x.AcademicYearName,
                    EducationStageName = x.EducationStageName,
                    EducationYearName = x.EducationYearName,
                    BillingType = x.BillingType,
                    StartDate = x.StartDate,
                    IsActive = x.IsActive,
                    AssignedGroupId = membership?.LessonGroupId,
                    AssignedGroupName = membership?.Name
                };
            })
            .ToList();

        return Result<IReadOnlyList<TeacherStudentLessonDto>>.Success(items);
    }
}
