namespace Academy.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Audience { get; set; } = null!;

    public int AccessTokenExpirationDays { get; set; } = 30;

    public int RefreshTokenExpirationDays { get; set; } = 365;
}
