using Academy.Application.Common.Localization;
using Academy.Application.Common.Images;
using Academy.Application.Features.Marketplace.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;

namespace Academy.Application.Features.Marketplace;

public static class MarketplaceMappings
{
    public const int BioPreviewLength = 160;

    public static string? PreviewBio(string? bio)
    {
        if (string.IsNullOrWhiteSpace(bio))
            return null;

        var trimmed = bio.Trim();
        return trimmed.Length <= BioPreviewLength
            ? trimmed
            : trimmed[..BioPreviewLength].TrimEnd() + "…";
    }

    public static TeacherReviewDto ToDto(TeacherReview review) => new()
    {
        Id = review.Id,
        TeacherId = review.TeacherId,
        StudentId = review.StudentId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAtUtc = review.CreatedAtUtc,
        UpdatedAtUtc = review.UpdatedAtUtc
    };

    public static PublicReviewDto ToPublicDto(TeacherReview review) => new()
    {
        Id = review.Id,
        StudentName = review.Student.User.FullName,
        StudentPhotoUrl = ImageService.DisplayValue(review.Student.User.ProfilePhoto),
        TeacherName = review.Teacher?.User is null ? null : review.Teacher.User.FullName,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAtUtc = review.CreatedAtUtc,
        UpdatedAtUtc = review.UpdatedAtUtc
    };

    public static PublicLessonCardDto ToLessonCard(
        Lesson lesson,
        AppLanguage language,
        LessonSeatAvailability seats,
        bool alreadyBooked,
        TeacherRatingSnapshot? rating = null)
    {
        var snapshot = rating ?? TeacherRatingCalculator.From(
            lesson.Teacher.Reviews?.Select(x => x.Rating) ?? []);
        var teacherName = lesson.Teacher.User.FullName;
        return new PublicLessonCardDto
        {
            Id = lesson.Id,
            TeacherId = lesson.TeacherId,
            TeacherName = teacherName,
            TeacherPhotoUrl = ImageService.DisplayValue(lesson.Teacher.User.ProfilePhoto),
            Subject = LocalizedNames.Pick(
                lesson.EducationSubject.NameAr,
                lesson.EducationSubject.NameEn,
                language),
            EducationTypeId = lesson.EducationTypeId,
            EducationTypeName = LocalizedNames.Pick(
                lesson.EducationType.NameAr,
                lesson.EducationType.NameEn,
                language),
            EducationStageId = lesson.EducationStageId,
            EducationStageName = LocalizedNames.Pick(
                lesson.EducationStage.NameAr,
                lesson.EducationStage.NameEn,
                language),
            EducationYearId = lesson.EducationYearId,
            EducationYearName = LocalizedNames.Pick(
                lesson.EducationYear.NameAr,
                lesson.EducationYear.NameEn,
                language),
            BillingType = lesson.BillingType.ToString(),
            SessionPrice = lesson.SessionPrice,
            MonthlyPrice = lesson.MonthlyPrice,
            StartDate = lesson.StartDate,
            CountryId = lesson.CountryId,
            CountryName = LocalizedNames.Pick(lesson.Country.NameAr, lesson.Country.NameEn, language),
            RemainingSeats = seats.RemainingSeats,
            SeatsOpen = seats.SeatsOpen,
            IsFull = seats.IsFull,
            TeacherRatingAverage = snapshot.Average,
            TeacherRatingCount = snapshot.Count,
            TeacherRatingStars = snapshot.Stars,
            AlreadyBooked = alreadyBooked
        };
    }
}
