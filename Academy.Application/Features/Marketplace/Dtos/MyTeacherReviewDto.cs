namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class MyTeacherReviewDto
{
    public required bool CanReview { get; init; }

    public TeacherReviewDto? Review { get; init; }
}
