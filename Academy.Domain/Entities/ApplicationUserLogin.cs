using Microsoft.AspNetCore.Identity;

namespace Academy.Domain.Entities;

public class ApplicationUserLogin : IdentityUserLogin<int>
{
    public ApplicationUser User { get; set; } = null!;
}
