using Academy.Application.Common.Models;
using Academy.Application.Common.Images;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace;
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
            .AsNoTracking()
            .Where(x => x.IsActive && !bookedLessonIds.Contains(x.Id))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AvailableLessonDto
            {
                Id = x.Id,
                TeacherId = x.TeacherId,
                TeacherName = (x.Teacher.User.FirstName + " " + x.Teacher.User.LastName).Trim(),
                TeacherPhotoUrl = x.Teacher.User.ProfilePhoto,
                Subject = language == AppLanguage.Arabic
                    ? x.EducationSubject.NameAr
                    : x.EducationSubject.NameEn,
                AcademicYearId = x.AcademicYearId,
                AcademicYearName = x.AcademicYear.Name,
                EducationStageId = x.EducationStageId,
                EducationStageName = language == AppLanguage.Arabic
                    ? x.EducationStage.NameAr
                    : x.EducationStage.NameEn,
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
                    : x.Country.NameEn,
                RemainingSeats = null,
                SeatsOpen = true,
                IsFull = false,
                TeacherRatingAverage = x.Teacher.Reviews.Select(r => (decimal)r.Rating).DefaultIfEmpty().Average(),
                TeacherRatingCount = x.Teacher.Reviews.Count()
            })
            .ToListAsync(cancellationToken);

        var seats = await LessonSeatLookup.ForLessonsAsync(
            dbContext,
            lessons.Select(x => x.Id),
            cancellationToken);

        var items = lessons
            .Select(lesson =>
            {
                var availability = seats.GetValueOrDefault(lesson.Id, LessonSeatAvailability.Open());
                return new AvailableLessonDto
                {
                    Id = lesson.Id,
                    TeacherId = lesson.TeacherId,
                    TeacherName = lesson.TeacherName,
                    TeacherPhotoUrl = ImageService.DisplayValue(lesson.TeacherPhotoUrl),
                    Subject = lesson.Subject,
                    AcademicYearId = lesson.AcademicYearId,
                    AcademicYearName = lesson.AcademicYearName,
                    EducationStageId = lesson.EducationStageId,
                    EducationStageName = lesson.EducationStageName,
                    EducationYearId = lesson.EducationYearId,
                    EducationYearName = lesson.EducationYearName,
                    BillingType = lesson.BillingType,
                    SessionPrice = lesson.SessionPrice,
                    MonthlyPrice = lesson.MonthlyPrice,
                    StartDate = lesson.StartDate,
                    CountryId = lesson.CountryId,
                    CountryName = lesson.CountryName,
                    RemainingSeats = availability.RemainingSeats,
                    SeatsOpen = availability.SeatsOpen,
                    IsFull = availability.IsFull,
                    TeacherRatingAverage = lesson.TeacherRatingAverage,
                    TeacherRatingCount = lesson.TeacherRatingCount
                };
            })
            .ToList();

        return Result<IReadOnlyList<AvailableLessonDto>>.Success(items);
    }
}
