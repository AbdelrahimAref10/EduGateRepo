using Academy.Domain.Enums;

namespace Academy.Application.Features.Auth.Dtos;

public sealed class RegisterRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public AppRole Role { get; set; }

    public int AreaId { get; set; }
}
