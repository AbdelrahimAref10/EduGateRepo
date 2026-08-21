namespace Academy.Application.Features.Account.Dtos;

public sealed class UpdateMyProfileRequest
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    public int AreaId { get; set; }

    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }

    public string? ConfirmNewPassword { get; set; }
}
