using Microsoft.AspNetCore.Identity;

namespace Academy.Domain.Entities;

public class ApplicationUserClaim : IdentityUserClaim<int>
{
    public ApplicationUser User { get; set; } = null!;
}
