using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationSubject;

public sealed class CreateEducationSubjectCommandValidator : AbstractValidator<CreateEducationSubjectCommand>
{
    public CreateEducationSubjectCommandValidator()
    {
        RuleFor(x => x.EducationStageId).GreaterThan(0);
        RuleFor(x => x.EducationYearId).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
