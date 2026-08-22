namespace Academy.Domain.Common;

public static class AppPermissions
{
    public const string ClaimType = "permission";

    /// <summary>
    /// Allows creating users, listing users, and assigning roles/permissions.
    /// Only meaningful for SuperAdmin accounts.
    /// </summary>
    public const string ManageUsers = "ManageUsers";
}

public static class AppPolicies
{
    public const string ManageUsers = "ManageUsers";
}
