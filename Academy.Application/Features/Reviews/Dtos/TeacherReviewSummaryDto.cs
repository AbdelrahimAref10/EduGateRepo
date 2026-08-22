namespace Academy.Application.Features.Reviews.Dtos;

public sealed class ReviewStatDto
{
    public required int Count { get; init; }

    public required decimal Average { get; init; }

    public required int Stars { get; init; }
}

public sealed class TeacherReviewSummaryDto
{
    public required ReviewStatDto All { get; init; }

    public required ReviewStatDto Teacher { get; init; }

    public required ReviewStatDto Lessons { get; init; }

    public required ReviewStatDto Sessions { get; init; }
}
