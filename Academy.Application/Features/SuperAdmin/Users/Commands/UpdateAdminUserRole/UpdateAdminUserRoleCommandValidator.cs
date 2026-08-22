using Academy.Domain.Enums;
using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Users.Commands.UpdateAdminUserRole;

public sealed class UpdateAdminUserRoleCommandValidator : AbstractValidator<UpdateAdminUserRoleCommand>
{
    public UpdateAdminUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(role => Enum.IsDefined(typeof(AppRole), role))
            .WithMessage("Invalid role.");
    }
}
