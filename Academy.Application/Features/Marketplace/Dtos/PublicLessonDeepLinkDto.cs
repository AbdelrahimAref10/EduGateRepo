namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class PublicLessonDeepLinkDto
{
    public required int LessonId { get; init; }

    public required int TeacherId { get; init; }
}
