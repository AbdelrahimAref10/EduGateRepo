using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationStage;

public sealed class CreateEducationStageCommandValidator : AbstractValidator<CreateEducationStageCommand>
{
    public CreateEducationStageCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
