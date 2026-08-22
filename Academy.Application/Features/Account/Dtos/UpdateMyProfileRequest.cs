namespace Academy.Application.Features.Account.Dtos;

public sealed class UpdateMyProfileRequest
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    /// <summary>
    /// Optional profile photo as base64 or a data URL. Empty string removes the photo.
    /// Null leaves the current photo unchanged.
    /// </summary>
    public string? PhotoBase64 { get; set; }

    public int AreaId { get; set; }

    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }

    public string? ConfirmNewPassword { get; set; }
}
