using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateArea;

public sealed class UpdateAreaCommandValidator : AbstractValidator<UpdateAreaCommand>
{
    public UpdateAreaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
    }
}
