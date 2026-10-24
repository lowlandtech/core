namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default implementation of <see cref="IRolePrivilegeDiscovery"/>.
/// Aggregates explicit <see cref="RolePrivilegeDescriptor"/> registrations from
/// <see cref="IRolePrivilegeRegistry"/> and also derives permissions dynamically
/// from <see cref="IPermissionRegistry"/> so the admin role automatically receives
/// every permission declared by installed plugins.
/// </summary>
public sealed class RolePrivilegeDiscovery(
    IEnumerable<IRolePrivilegeRegistry> registries,
    IPermissionRegistry permissions) : IRolePrivilegeDiscovery
{
    public IReadOnlyList<string> GetPermissionsForRole(string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName);

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Collect from explicit role privilege descriptors
        foreach (var registry in registries)
        {
            foreach (var descriptor in registry.All)
            {
                if (string.Equals(descriptor.RoleName, roleName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var key in descriptor.PermissionKeys)
                        result.Add(key);
                }
            }
        }

        // 2. Admin role automatically gets ALL registered permissions
        if (string.Equals(roleName, "admin", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var perm in permissions.All)
                result.Add(perm.Key);
        }

        return result.ToList();
    }
}
