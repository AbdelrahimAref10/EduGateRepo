using Academy.Domain.Enums;
using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Commands.CreateMakeupSession;

public sealed class CreateMakeupSessionCommandValidator : AbstractValidator<CreateMakeupSessionCommand>
{
    public CreateMakeupSessionCommandValidator()
    {
        RuleFor(x => x.StudentIds).NotEmpty();
        RuleFor(x => x.GroupId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);

        When(x => !x.IsFree, () =>
        {
            // Per-session amount is filled from SessionPrice in the handler.
            // Monthly requires an explicit amount.
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .When(x => x.Amount is not null);

            RuleFor(x => x.Settlement)
                .Must(s =>
                    s is ChargeSettlement.Standalone
                        or ChargeSettlement.CurrentCycle
                        or ChargeSettlement.NextCycle
                        or ChargeSettlement.None)
                .WithMessage("تسوية غير صالحة.");
        });
    }
}
