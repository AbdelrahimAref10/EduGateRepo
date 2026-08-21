using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsActive => RevokedAtUtc is null && !IsExpired;
}
