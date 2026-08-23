using Academy.Application.Features.Teacher.Billing.Commands.RecordPayment;
using Academy.Domain.Enums;
using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Commands.RecordPayment;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
    }
}
