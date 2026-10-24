namespace LowlandTech.Core.Plugins;

/// <summary>
/// Descriptor that maps a role name to a set of permission keys.
/// Part of the DRD (Descriptor, Registry, Discovery) pattern for role privileges.
/// </summary>
public sealed record RolePrivilegeDescriptor(
    string RoleName,
    IReadOnlyList<string> PermissionKeys,
    string? PluginId = null
);
