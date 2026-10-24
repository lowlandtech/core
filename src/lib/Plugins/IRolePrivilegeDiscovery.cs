namespace LowlandTech.Core.Plugins;

/// <summary>
/// Aggregates multiple <see cref="IRolePrivilegeRegistry"/> instances to discover
/// the effective permission keys for a given role name.
/// Part of the DRD (Descriptor, Registry, Discovery) pattern for role privileges.
/// </summary>
public interface IRolePrivilegeDiscovery
{
    /// <summary>
    /// Returns the distinct permission keys that should be granted to <paramref name="roleName"/>
    /// by default, aggregated across all registered role privilege registries.
    /// </summary>
    IReadOnlyList<string> GetPermissionsForRole(string roleName);
}
