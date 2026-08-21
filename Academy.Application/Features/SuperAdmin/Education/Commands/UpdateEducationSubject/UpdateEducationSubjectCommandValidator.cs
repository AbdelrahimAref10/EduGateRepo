using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationSubject;

public sealed class UpdateEducationSubjectCommandValidator : AbstractValidator<UpdateEducationSubjectCommand>
{
    public UpdateEducationSubjectCommandValidator()
    {
        RuleFor(x => x.EducationTypeId).GreaterThan(0);
        RuleFor(x => x.EducationStageId).GreaterThan(0);
        RuleFor(x => x.EducationYearId).GreaterThan(0);
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
