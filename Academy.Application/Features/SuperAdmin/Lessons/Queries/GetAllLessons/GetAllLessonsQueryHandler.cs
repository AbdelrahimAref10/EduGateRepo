using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Lessons.Queries.GetAllLessons;

public sealed class GetAllLessonsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetAllLessonsQuery, Result<IReadOnlyList<AdminLessonListItemDto>>>
{
    public async Task<Result<IReadOnlyList<AdminLessonListItemDto>>> Handle(
        GetAllLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var language = requestLanguage.Current;

        var lessons = await dbContext.Lessons
            .AsNoTracking()
            .Include(x => x.Teacher)
                .ThenInclude(x => x.User)
            .Include(x => x.AcademicYear)
            .Include(x => x.EducationStage)
            .Include(x => x.EducationYear)
            .Include(x => x.EducationSubject)
            .Include(x => x.Bookings)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Members)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var items = lessons.Select(lesson =>
        {
            var assignments = lesson.Groups
                .SelectMany(g => g.Members.Select(m => new { m.StudentId, Group = g }))
                .GroupBy(x => x.StudentId)
                .ToDictionary(g => g.Key, g => g.First().Group);

            var students = lesson.Bookings
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x =>
                {
                    assignments.TryGetValue(x.StudentId, out var group);
                    return new AdminLessonStudentDto
                    {
                        BookingId = x.Id,
                        StudentId = x.StudentId,
                        StudentName = x.Student.User.FullName,
                        StudentCode = x.Student.StudentCode,
                        Status = x.Status.ToString(),
                        CreatedAtUtc = x.CreatedAtUtc,
                        ReviewedAtUtc = x.ReviewedAtUtc,
                        AssignedGroupId = group?.Id,
                        AssignedGroupName = group?.Name
                    };
                })
                .ToList();

            return new AdminLessonListItemDto
            {
                Id = lesson.Id,
                Subject = LocalizedNames.Pick(
                    lesson.EducationSubject.NameAr,
                    lesson.EducationSubject.NameEn,
                    language),
                TeacherId = lesson.TeacherId,
                TeacherName = lesson.Teacher.User.FullName,
                AcademicYearName = lesson.AcademicYear.Name,
                EducationStageName = LocalizedNames.Pick(
                    lesson.EducationStage.NameAr,
                    lesson.EducationStage.NameEn,
                    language),
                EducationYearName = LocalizedNames.Pick(
                    lesson.EducationYear.NameAr,
                    lesson.EducationYear.NameEn,
                    language),
                IsActive = lesson.IsActive,
                StartedAtUtc = lesson.StartedAtUtc,
                HasStarted = lesson.StartedAtUtc.HasValue,
                GroupsCount = lesson.Groups.Count,
                BookingsCount = lesson.Bookings.Count,
                ConfirmedBookingsCount = lesson.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CreatedAtUtc = lesson.CreatedAtUtc,
                Students = students
            };
        }).ToList();

        return Result<IReadOnlyList<AdminLessonListItemDto>>.Success(items);
    }
}
