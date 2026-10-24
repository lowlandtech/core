namespace LowlandTech.Core.Plugins;

/// <summary>
/// Thread-safe in-memory registry for <see cref="RolePrivilegeDescriptor"/> entries.
/// Part of the DRD pattern for role privileges.
/// </summary>
public sealed class RolePrivilegeRegistry
    : DescriptorRegistry<string, RolePrivilegeDescriptor>, IRolePrivilegeRegistry
{
    public RolePrivilegeRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(RolePrivilegeDescriptor descriptor)
        => $"{descriptor.RoleName}:{string.Join(",", descriptor.PermissionKeys)}";

    protected override void OnDuplicate(string key, RolePrivilegeDescriptor existing, RolePrivilegeDescriptor incoming)
    {
        // Idempotent: same role + permissions combination is fine
    }

    public void RemoveByPlugin(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        RemoveWhere(d => string.Equals(d.PluginId, pluginId, StringComparison.OrdinalIgnoreCase));
    }
}
