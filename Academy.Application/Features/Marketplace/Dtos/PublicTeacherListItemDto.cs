namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class PublicTeacherListItemDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string? Bio { get; init; }

    public required decimal RatingAverage { get; init; }

    public required int RatingCount { get; init; }

    public required int RatingStars { get; init; }

    public required int ActiveLessonsCount { get; init; }

    public string? CountryName { get; init; }

    public string? SubjectName { get; init; }

    public string? PhotoUrl { get; init; }
}
