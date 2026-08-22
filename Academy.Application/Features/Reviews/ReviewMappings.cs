using Academy.Application.Features.Marketplace;
using Academy.Application.Features.Reviews.Dtos;
using Academy.Domain.Entities;

namespace Academy.Application.Features.Reviews;

internal static class ReviewMappings
{
    public static TargetReviewDto ToDto(LessonReview review) => new()
    {
        Id = review.Id,
        TargetId = review.LessonId,
        StudentId = review.StudentId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAtUtc = review.CreatedAtUtc,
        UpdatedAtUtc = review.UpdatedAtUtc
    };

    public static TargetReviewDto ToDto(SessionReview review) => new()
    {
        Id = review.Id,
        TargetId = review.LessonGroupSessionId,
        StudentId = review.StudentId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAtUtc = review.CreatedAtUtc,
        UpdatedAtUtc = review.UpdatedAtUtc
    };

    public static ReviewStatDto ToStat(int count, double average)
    {
        if (count <= 0)
            return new ReviewStatDto { Count = 0, Average = 0, Stars = 0 };

        var rounded = Math.Round((decimal)average, 2, MidpointRounding.AwayFromZero);
        return new ReviewStatDto
        {
            Count = count,
            Average = rounded,
            Stars = TeacherRatingCalculator.FilledStars(rounded, count)
        };
    }

    public static void Apply(Lesson lesson, IReadOnlyCollection<int> ratings)
    {
        var snapshot = TeacherRatingCalculator.From(ratings);
        lesson.RatingAverage = snapshot.Average;
        lesson.RatingCount = snapshot.Count;
    }

    public static void Apply(LessonGroupSession session, IReadOnlyCollection<int> ratings)
    {
        var snapshot = TeacherRatingCalculator.From(ratings);
        session.RatingAverage = snapshot.Average;
        session.RatingCount = snapshot.Count;
    }

    public static string? TrimComment(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
}
