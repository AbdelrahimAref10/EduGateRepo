using Microsoft.AspNetCore.Identity;

namespace Academy.Domain.Entities;

public class ApplicationUserToken : IdentityUserToken<int>
{
    public ApplicationUser User { get; set; } = null!;
}
