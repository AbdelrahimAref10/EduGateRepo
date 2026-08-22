namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class PublicReviewDto
{
    public required int Id { get; init; }

    public required string StudentName { get; init; }

    public string? StudentPhotoUrl { get; init; }

    public string? TeacherName { get; init; }

    public required int Rating { get; init; }

    public string? Comment { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
