using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Lessons.Queries.GetAvailableLessons;

public sealed class GetAvailableLessonsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetAvailableLessonsQuery, Result<IReadOnlyList<AvailableLessonDto>>>
{
    public async Task<Result<IReadOnlyList<AvailableLessonDto>>> Handle(
        GetAvailableLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<IReadOnlyList<AvailableLessonDto>>.NotFound("Student profile was not found.");

        var bookedLessonIds = dbContext.LessonBookings
            .Where(x => x.StudentId == student.Id)
            .Select(x => x.LessonId);

        var language = requestLanguage.Current;

        var lessons = await dbContext.Lessons
            .Where(x => x.IsActive && !bookedLessonIds.Contains(x.Id))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AvailableLessonDto
            {
                Id = x.Id,
                TeacherId = x.TeacherId,
                TeacherName = (x.Teacher.User.FirstName + " " + x.Teacher.User.LastName).Trim(),
                Subject = x.Subject,
                EducationTypeId = x.EducationTypeId,
                EducationTypeName = language == AppLanguage.Arabic
                    ? x.EducationType.NameAr
                    : x.EducationType.NameEn,
                EducationYearId = x.EducationYearId,
                EducationYearName = language == AppLanguage.Arabic
                    ? x.EducationYear.NameAr
                    : x.EducationYear.NameEn,
                BillingType = x.BillingType.ToString(),
                SessionPrice = x.SessionPrice,
                MonthlyPrice = x.MonthlyPrice,
                StartDate = x.StartDate,
                CountryId = x.CountryId,
                CountryName = language == AppLanguage.Arabic
                    ? x.Country.NameAr
                    : x.Country.NameEn
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AvailableLessonDto>>.Success(lessons);
    }
}
