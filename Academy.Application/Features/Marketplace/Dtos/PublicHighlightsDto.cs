namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class PublicHighlightsDto
{
    public required IReadOnlyList<PublicTeacherListItemDto> Teachers { get; init; }

    public required IReadOnlyList<PublicLessonCardDto> Lessons { get; init; }

    public required IReadOnlyList<PublicReviewDto> Reviews { get; init; }
}
