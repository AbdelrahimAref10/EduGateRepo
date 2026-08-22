using Academy.Domain.Enums;
using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Users.Commands.CreateAdminUser;

public sealed class CreateAdminUserCommandValidator : AbstractValidator<CreateAdminUserCommand>
{
    public CreateAdminUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Password and confirm password do not match.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(role => Enum.IsDefined(typeof(AppRole), role))
            .WithMessage("Invalid role.");

        RuleFor(x => x.AreaId)
            .GreaterThan(0)
            .When(x => x.Role is not AppRole.SuperAdmin)
            .WithMessage("Area is required for Teacher, Student, and Parent.");
    }
}
