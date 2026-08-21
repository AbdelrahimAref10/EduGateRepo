using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationStage;

public sealed class UpdateEducationStageCommandValidator : AbstractValidator<UpdateEducationStageCommand>
{
    public UpdateEducationStageCommandValidator()
    {
        RuleFor(x => x.EducationTypeId).GreaterThan(0);
        RuleFor(x => x.StageId).GreaterThan(0);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
