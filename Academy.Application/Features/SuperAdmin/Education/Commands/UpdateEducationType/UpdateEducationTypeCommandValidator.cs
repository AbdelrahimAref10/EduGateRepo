using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationType;

public sealed class UpdateEducationTypeCommandValidator : AbstractValidator<UpdateEducationTypeCommand>
{
    public UpdateEducationTypeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
