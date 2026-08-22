using FluentValidation;

namespace Academy.Application.Features.Marketplace.Commands.CreateTeacherReview;

public sealed class CreateTeacherReviewCommandValidator : AbstractValidator<CreateTeacherReviewCommand>
{
    public CreateTeacherReviewCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
