using Academy.Application.Common.Localization;
using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicTeacher;

public sealed class GetPublicTeacherQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetPublicTeacherQuery, Result<PublicTeacherDetailDto>>
{
    public async Task<Result<PublicTeacherDetailDto>> Handle(
        GetPublicTeacherQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .AsNoTracking()
            .Include(x => x.User)
                .ThenInclude(x => x.Area!)
                    .ThenInclude(x => x.City)
                        .ThenInclude(x => x.Governorate)
                            .ThenInclude(x => x.Country)
            .FirstOrDefaultAsync(x => x.Id == request.TeacherId, cancellationToken);

        if (teacher is null)
            return Result<PublicTeacherDetailDto>.NotFound("Teacher was not found.");

        var language = requestLanguage.Current;

        var lessons = await dbContext.Lessons
            .AsNoTracking()
            .Include(x => x.Teacher)
                .ThenInclude(x => x.User)
            .Include(x => x.EducationType)
            .Include(x => x.EducationStage)
            .Include(x => x.EducationYear)
            .Include(x => x.EducationSubject)
            .Include(x => x.Country)
            .Where(x => x.TeacherId == teacher.Id && x.IsActive)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var seats = await LessonSeatLookup.ForLessonsAsync(
            dbContext,
            lessons.Select(l => l.Id),
            cancellationToken);

        var ratingValues = await dbContext.TeacherReviews
            .AsNoTracking()
            .Where(x => x.TeacherId == teacher.Id)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);
        var rating = TeacherRatingCalculator.From(ratingValues);

        var reviews = await dbContext.TeacherReviews
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Where(x => x.TeacherId == teacher.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        var isOwnProfile = false;
        var canReview = false;
        TeacherReviewDto? myReview = null;
        var bookedLessonIds = new HashSet<int>();

        if (request.ViewerUserId is int viewerUserId)
        {
            isOwnProfile = teacher.UserId == viewerUserId;

            var student = await dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UserId == viewerUserId && !x.IsParent,
                    cancellationToken);

            if (student is not null)
            {
                canReview = await dbContext.LessonBookings.AnyAsync(
                    x => x.StudentId == student.Id
                        && x.TeacherId == teacher.Id
                        && x.Status == BookingStatus.Confirmed,
                    cancellationToken);

                bookedLessonIds = (await dbContext.LessonBookings
                    .Where(x => x.StudentId == student.Id && x.TeacherId == teacher.Id)
                    .Select(x => x.LessonId)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();

                var existing = reviews.FirstOrDefault(x => x.StudentId == student.Id)
                    ?? await dbContext.TeacherReviews
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            x => x.TeacherId == teacher.Id && x.StudentId == student.Id,
                            cancellationToken);

                if (existing is not null)
                    myReview = MarketplaceMappings.ToDto(existing);
            }
        }

        var area = teacher.User.Area;
        var countryName = area is not null
            ? LocalizedNames.Pick(area.City.Governorate.Country.NameAr, area.City.Governorate.Country.NameEn, language)
            : lessons
                .Select(l => LocalizedNames.Pick(l.Country.NameAr, l.Country.NameEn, language))
                .FirstOrDefault();

        var areaName = area is not null
            ? LocalizedNames.Pick(area.NameAr, area.NameEn, language)
            : null;

        return Result<PublicTeacherDetailDto>.Success(new PublicTeacherDetailDto
        {
            Id = teacher.Id,
            Name = teacher.User.FullName,
            PhotoUrl = ImageService.DisplayValue(teacher.User.ProfilePhoto),
            Bio = string.IsNullOrWhiteSpace(teacher.User.Bio) ? null : teacher.User.Bio.Trim(),
            RatingAverage = rating.Average,
            RatingCount = rating.Count,
            RatingStars = rating.Stars,
            CountryName = countryName,
            AreaName = areaName,
            IsOwnProfile = isOwnProfile,
            CanReview = canReview,
            MyReview = myReview,
            Lessons = lessons
                .Select(l => MarketplaceMappings.ToLessonCard(
                    l,
                    language,
                    seats.GetValueOrDefault(l.Id, LessonSeatAvailability.Open()),
                    bookedLessonIds.Contains(l.Id),
                    rating))
                .ToList(),
            Reviews = reviews.Select(MarketplaceMappings.ToPublicDto).ToList()
        });
    }
}
