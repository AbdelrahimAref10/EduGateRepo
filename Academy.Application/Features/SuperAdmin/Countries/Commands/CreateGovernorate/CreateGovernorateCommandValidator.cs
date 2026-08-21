using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateGovernorate;

public sealed class CreateGovernorateCommandValidator : AbstractValidator<CreateGovernorateCommand>
{
    public CreateGovernorateCommandValidator()
    {
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
    }
}
