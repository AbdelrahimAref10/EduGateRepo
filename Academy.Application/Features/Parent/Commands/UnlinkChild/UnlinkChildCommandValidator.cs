using FluentValidation;

namespace Academy.Application.Features.Parent.Commands.UnlinkChild;

public sealed class UnlinkChildCommandValidator : AbstractValidator<UnlinkChildCommand>
{
    public UnlinkChildCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ChildStudentId).GreaterThan(0);
    }
}
