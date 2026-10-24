namespace LowlandTech.Core.Plugins;

/// <summary>
/// Registry that owns <see cref="RolePrivilegeDescriptor"/> entries.
/// Part of the DRD pattern for role privileges.
/// SPEC2050 US09 extension.
/// </summary>
public interface IRolePrivilegeRegistry
{
    IReadOnlyCollection<RolePrivilegeDescriptor> All { get; }

    /// <summary>Register a role privilege descriptor.</summary>
    void Register(RolePrivilegeDescriptor descriptor);

    /// <summary>Remove all descriptors contributed by a plugin (uninstall path).</summary>
    void RemoveByPlugin(string pluginId);
}
