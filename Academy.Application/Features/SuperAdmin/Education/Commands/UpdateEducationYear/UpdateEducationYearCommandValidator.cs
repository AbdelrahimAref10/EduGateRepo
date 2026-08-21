using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationYear;

public sealed class UpdateEducationYearCommandValidator : AbstractValidator<UpdateEducationYearCommand>
{
    public UpdateEducationYearCommandValidator()
    {
        RuleFor(x => x.EducationTypeId).GreaterThan(0);
        RuleFor(x => x.YearId).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
