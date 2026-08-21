using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationType;

public sealed class CreateEducationTypeCommandValidator : AbstractValidator<CreateEducationTypeCommand>
{
    public CreateEducationTypeCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
