using FluentValidation;

namespace Academy.Application.Features.Reviews.Commands.UpsertLessonReview;

public sealed class UpsertLessonReviewCommandValidator : AbstractValidator<UpsertLessonReviewCommand>
{
    public UpsertLessonReviewCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
