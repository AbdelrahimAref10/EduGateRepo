using Academy.Domain.Enums;

namespace Academy.Application.Contracts.Identity;

public sealed class TokenUserInfo
{
    public required int UserId { get; init; }

    public required string Email { get; init; }

    public required string FullName { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public IReadOnlyList<string> Permissions { get; init; } = [];

    public AppLanguage LanguageId { get; init; } = AppLanguage.Arabic;
}

public sealed class GeneratedTokens
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTime AccessTokenExpiresAtUtc { get; init; }

    public required DateTime RefreshTokenExpiresAtUtc { get; init; }
}

public interface ITokenService
{
    GeneratedTokens GenerateTokens(TokenUserInfo user);

    string HashRefreshToken(string refreshToken);
}
