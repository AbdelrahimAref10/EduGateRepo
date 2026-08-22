using Academy.Application.Common.Localization;
using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicTeachers;

public sealed class GetPublicTeachersQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetPublicTeachersQuery, Result<IReadOnlyList<PublicTeacherListItemDto>>>
{
    public async Task<Result<IReadOnlyList<PublicTeacherListItemDto>>> Handle(
        GetPublicTeachersQuery request,
        CancellationToken cancellationToken)
    {
        var language = requestLanguage.Current;

        var query = dbContext.Teachers
            .AsNoTracking()
            .Where(t => t.Lessons.Any(l =>
                l.IsActive
                && (request.CountryId == null || l.CountryId == request.CountryId)
                && (request.EducationStageId == null || l.EducationStageId == request.EducationStageId)
                && (request.EducationSubjectId == null || l.EducationSubjectId == request.EducationSubjectId)));

        var teachers = await query
            .OrderByDescending(t => t.Reviews.Select(r => r.Rating).DefaultIfEmpty().Average())
            .ThenByDescending(t => t.Reviews.Count())
            .ThenBy(t => t.User.FirstName)
            .ThenBy(t => t.User.LastName)
            .Select(t => new PublicTeacherListItemDto
            {
                Id = t.Id,
                Name = (t.User.FirstName + " " + t.User.LastName).Trim(),
                Bio = t.User.Bio,
                RatingAverage = t.Reviews.Select(r => (decimal)r.Rating).DefaultIfEmpty().Average(),
                RatingCount = t.Reviews.Count(),
                RatingStars = 0,
                ActiveLessonsCount = t.Lessons.Count(l =>
                    l.IsActive
                    && (request.CountryId == null || l.CountryId == request.CountryId)
                    && (request.EducationStageId == null || l.EducationStageId == request.EducationStageId)
                    && (request.EducationSubjectId == null || l.EducationSubjectId == request.EducationSubjectId)),
                CountryName = t.User.Area != null
                    ? (language == Domain.Enums.AppLanguage.Arabic
                        ? t.User.Area.City.Governorate.Country.NameAr
                        : t.User.Area.City.Governorate.Country.NameEn)
                    : t.Lessons
                        .Where(l => l.IsActive)
                        .Select(l => language == Domain.Enums.AppLanguage.Arabic
                            ? l.Country.NameAr
                            : l.Country.NameEn)
                        .FirstOrDefault(),
                SubjectName = t.Lessons
                    .Where(l =>
                        l.IsActive
                        && (request.CountryId == null || l.CountryId == request.CountryId)
                        && (request.EducationStageId == null || l.EducationStageId == request.EducationStageId)
                        && (request.EducationSubjectId == null || l.EducationSubjectId == request.EducationSubjectId))
                    .Select(l => language == Domain.Enums.AppLanguage.Arabic
                        ? l.EducationSubject.NameAr
                        : l.EducationSubject.NameEn)
                    .FirstOrDefault(),
                PhotoUrl = t.User.ProfilePhoto
            })
            .ToListAsync(cancellationToken);

        var items = teachers
            .Select(t => new PublicTeacherListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Bio = MarketplaceMappings.PreviewBio(t.Bio),
                RatingAverage = t.RatingAverage,
                RatingCount = t.RatingCount,
                RatingStars = TeacherRatingCalculator.FilledStars(t.RatingAverage, t.RatingCount),
                ActiveLessonsCount = t.ActiveLessonsCount,
                CountryName = t.CountryName,
                SubjectName = t.SubjectName,
                PhotoUrl = ImageService.DisplayValue(t.PhotoUrl)
            })
            .ToList();

        return Result<IReadOnlyList<PublicTeacherListItemDto>>.Success(items);
    }
}
