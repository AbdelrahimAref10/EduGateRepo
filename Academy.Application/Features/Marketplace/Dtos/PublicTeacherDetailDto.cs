namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class PublicTeacherDetailDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string? Bio { get; init; }

    public required decimal RatingAverage { get; init; }

    public required int RatingCount { get; init; }

    public required int RatingStars { get; init; }

    public string? CountryName { get; init; }

    public string? AreaName { get; init; }

    public string? PhotoUrl { get; init; }

    public required bool IsOwnProfile { get; init; }

    public required bool CanReview { get; init; }

    public TeacherReviewDto? MyReview { get; init; }

    public required IReadOnlyList<PublicLessonCardDto> Lessons { get; init; }

    public required IReadOnlyList<PublicReviewDto> Reviews { get; init; }
}
