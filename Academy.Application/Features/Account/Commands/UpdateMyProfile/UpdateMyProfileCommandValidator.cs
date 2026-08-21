using FluentValidation;

namespace Academy.Application.Features.Account.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.AreaId).GreaterThan(0);

        RuleFor(x => x)
            .Must(x =>
                string.IsNullOrWhiteSpace(x.CurrentPassword)
                && string.IsNullOrWhiteSpace(x.NewPassword)
                && string.IsNullOrWhiteSpace(x.ConfirmNewPassword)
                || (!string.IsNullOrWhiteSpace(x.CurrentPassword)
                    && !string.IsNullOrWhiteSpace(x.NewPassword)
                    && !string.IsNullOrWhiteSpace(x.ConfirmNewPassword)))
            .WithMessage("To change password, provide current password, new password, and confirmation.");

        RuleFor(x => x.NewPassword)
            .MinimumLength(6)
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword))
            .WithMessage("New password and confirmation do not match.");
    }
}
