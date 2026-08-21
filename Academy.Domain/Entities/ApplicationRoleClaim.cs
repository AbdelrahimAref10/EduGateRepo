using Microsoft.AspNetCore.Identity;

namespace Academy.Domain.Entities;

public class ApplicationRoleClaim : IdentityRoleClaim<int>
{
    public ApplicationRole Role { get; set; } = null!;
}
