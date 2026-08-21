namespace Academy.Application.Features.Auth.Dtos;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}
