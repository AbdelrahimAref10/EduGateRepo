using Microsoft.AspNetCore.Identity;

namespace Academy.Domain.Entities;

public class ApplicationRole : IdentityRole<int>
{
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

    public ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = [];
}
