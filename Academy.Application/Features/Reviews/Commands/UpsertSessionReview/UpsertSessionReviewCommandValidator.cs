using FluentValidation;

namespace Academy.Application.Features.Reviews.Commands.UpsertSessionReview;

public sealed class UpsertSessionReviewCommandValidator : AbstractValidator<UpsertSessionReviewCommand>
{
    public UpsertSessionReviewCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
