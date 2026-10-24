namespace LowlandTech.Core;

public static class TenantRoles
{
    public const string Owner = "Owner";
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Member = "Member";

    public static bool CanManageGroups(string role) => role is Owner or SuperAdmin;
    public static bool CanManageMembers(string role) => role is Owner or SuperAdmin or Admin;
    public static bool IsAtLeast(string role, string requiredRole) => Rank(role) >= Rank(requiredRole);

    private static int Rank(string role) => role switch
    {
        Owner => 4,
        SuperAdmin => 3,
        Admin => 2,
        Member => 1,
        _ => 0
    };
}

public static class GroupRoles
{
    public const string Admin = "Admin";
    public const string Member = "Member";
}
