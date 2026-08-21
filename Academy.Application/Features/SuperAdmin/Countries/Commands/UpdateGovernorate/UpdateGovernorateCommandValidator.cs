using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateGovernorate;

public sealed class UpdateGovernorateCommandValidator : AbstractValidator<UpdateGovernorateCommand>
{
    public UpdateGovernorateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
    }
}
