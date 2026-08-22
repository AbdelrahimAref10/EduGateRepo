namespace Academy.Application.Features.Auth.Dtos;

public sealed class AuthResponseDto
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTime AccessTokenExpiresAtUtc { get; init; }

    public required DateTime RefreshTokenExpiresAtUtc { get; init; }

    public required int UserId { get; init; }

    public required string Email { get; init; }

    public required string FullName { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>
    /// Preferred language id: 1 = Arabic, 2 = English.
    /// </summary>
    public required int LanguageId { get; init; }

    public string? StudentCode { get; init; }

    public int? AreaId { get; init; }

    public string? PhotoUrl { get; init; }
}
