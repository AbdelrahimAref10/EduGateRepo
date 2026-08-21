using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateCity;

public sealed class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
{
    public UpdateCityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
    }
}
