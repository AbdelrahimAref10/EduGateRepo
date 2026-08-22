using FluentValidation;

namespace Academy.Application.Features.Marketplace.Commands.UpdateTeacherReview;

public sealed class UpdateTeacherReviewCommandValidator : AbstractValidator<UpdateTeacherReviewCommand>
{
    public UpdateTeacherReviewCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
