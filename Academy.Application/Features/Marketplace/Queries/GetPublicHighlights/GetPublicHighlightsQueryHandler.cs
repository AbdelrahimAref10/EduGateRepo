using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicHighlights;

public sealed class GetPublicHighlightsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetPublicHighlightsQuery, Result<PublicHighlightsDto>>
{
    public async Task<Result<PublicHighlightsDto>> Handle(
        GetPublicHighlightsQuery request,
        CancellationToken cancellationToken)
    {
        var teachersResult = await new GetPublicTeachers.GetPublicTeachersQueryHandler(dbContext, requestLanguage)
            .Handle(new GetPublicTeachers.GetPublicTeachersQuery(null, null, null), cancellationToken);

        var teachers = (teachersResult.Value ?? [])
            .Take(8)
            .ToList();

        var language = requestLanguage.Current;

        var lessonEntities = await dbContext.Lessons
            .AsNoTracking()
            .Include(x => x.Teacher)
                .ThenInclude(x => x.User)
            .Include(x => x.EducationType)
            .Include(x => x.EducationStage)
            .Include(x => x.EducationYear)
            .Include(x => x.EducationSubject)
            .Include(x => x.Country)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Teacher.RatingAverage)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(8)
            .ToListAsync(cancellationToken);

        var seats = await LessonSeatLookup.ForLessonsAsync(
            dbContext,
            lessonEntities.Select(x => x.Id),
            cancellationToken);

        var teacherIds = lessonEntities.Select(l => l.TeacherId).Distinct().ToList();
        var ratingRows = await dbContext.TeacherReviews
            .AsNoTracking()
            .Where(x => teacherIds.Contains(x.TeacherId))
            .Select(x => new { x.TeacherId, x.Rating })
            .ToListAsync(cancellationToken);

        var ratings = ratingRows
            .GroupBy(x => x.TeacherId)
            .ToDictionary(
                g => g.Key,
                g => TeacherRatingCalculator.From(g.Select(x => x.Rating)));

        var lessons = lessonEntities
            .Select(lesson => MarketplaceMappings.ToLessonCard(
                lesson,
                language,
                seats.GetValueOrDefault(lesson.Id, LessonSeatAvailability.Open()),
                false,
                ratings.GetValueOrDefault(lesson.TeacherId)))
            .ToList();

        var reviews = await dbContext.TeacherReviews
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Include(x => x.Teacher)
                .ThenInclude(x => x.User)
            .Where(x => x.Comment != null && x.Comment != "")
            .OrderByDescending(x => x.Rating)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(8)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            reviews = await dbContext.TeacherReviews
                .AsNoTracking()
                .Include(x => x.Student)
                    .ThenInclude(x => x.User)
                .Include(x => x.Teacher)
                    .ThenInclude(x => x.User)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(8)
                .ToListAsync(cancellationToken);
        }

        return Result<PublicHighlightsDto>.Success(new PublicHighlightsDto
        {
            Teachers = teachers,
            Lessons = lessons,
            Reviews = reviews.Select(MarketplaceMappings.ToPublicDto).ToList()
        });
    }
}
