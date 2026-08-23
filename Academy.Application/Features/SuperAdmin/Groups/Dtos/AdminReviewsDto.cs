namespace Academy.Application.Features.SuperAdmin.Groups.Dtos;

public sealed class AdminReviewDto
{
    public required int Id { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required int Rating { get; init; }

    public string? Comment { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed class AdminReviewsDto
{
    public required int TargetId { get; init; }

    public required int Count { get; init; }

    public required decimal Average { get; init; }

    public IReadOnlyList<AdminReviewDto> Items { get; init; } = [];
}
