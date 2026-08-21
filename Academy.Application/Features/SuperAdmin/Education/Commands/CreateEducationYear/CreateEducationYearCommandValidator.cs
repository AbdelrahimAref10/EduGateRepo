using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationYear;

public sealed class CreateEducationYearCommandValidator : AbstractValidator<CreateEducationYearCommand>
{
    public CreateEducationYearCommandValidator()
    {
        RuleFor(x => x.EducationTypeId).GreaterThan(0);
        RuleFor(x => x.EducationStageId).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
