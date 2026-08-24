using FluentValidation;

namespace Academy.Application.Features.Parent.Commands.LinkChild;

public sealed class LinkChildCommandValidator : AbstractValidator<LinkChildCommand>
{
    public LinkChildCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.StudentCode)
            .NotEmpty()
            .MaximumLength(32);
    }
}
