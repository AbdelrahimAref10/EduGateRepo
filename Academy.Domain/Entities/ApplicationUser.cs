using Academy.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Academy.Domain.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Bio { get; set; }

    /// <summary>
    /// Preferred UI language (Arabic = 1, English = 2).
    /// </summary>
    public AppLanguage PreferredLanguage { get; set; } = AppLanguage.Arabic;

    public int? AreaId { get; set; }

    public Area? Area { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationUserClaim> Claims { get; set; } = [];

    public ICollection<ApplicationUserLogin> Logins { get; set; } = [];

    public ICollection<ApplicationUserToken> Tokens { get; set; } = [];

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

    public Student? StudentProfile { get; set; }

    public Teacher? TeacherProfile { get; set; }

    public SuperAdmin? SuperAdminProfile { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public ICollection<NotificationDetail> NotificationDetails { get; set; } = [];

    public string FullName => $"{FirstName} {LastName}".Trim();
}
