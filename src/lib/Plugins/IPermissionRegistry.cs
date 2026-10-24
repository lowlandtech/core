
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks <see cref="PermissionDescriptor"/> contributions and validates that
/// every <see cref="FeatureDescriptor.RequiredPermissions"/> reference resolves
/// to a known key.
/// SPEC2050 US09.
/// </summary>
public interface IPermissionRegistry
{
    IReadOnlyCollection<PermissionDescriptor> All { get; }

    bool IsRegistered(string permissionKey);

    /// <exception cref="DuplicatePluginIdException">A descriptor with the same key is already registered.</exception>
    void Register(PermissionDescriptor descriptor);

    /// <summary>Remove all permissions contributed by a plugin (uninstall path).</summary>
    void RemoveByPlugin(string pluginId);

    /// <summary>Throws if any of the supplied permission keys are unknown.</summary>
    /// <exception cref="UnknownPermissionException">First missing key encountered.</exception>
    void EnsureAllRegistered(IEnumerable<string> permissionKeys);
}

public sealed class PermissionRegistry : DescriptorRegistry<string, PermissionDescriptor>, IPermissionRegistry
{
    public PermissionRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(PermissionDescriptor descriptor) => descriptor.Key;

    protected override void OnDuplicate(string key, PermissionDescriptor existing, PermissionDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    public void RemoveByPlugin(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        RemoveWhere(d => string.Equals(d.PluginId, pluginId, StringComparison.OrdinalIgnoreCase));
    }

    public void EnsureAllRegistered(IEnumerable<string> permissionKeys)
    {
        ArgumentNullException.ThrowIfNull(permissionKeys);
        foreach (var key in permissionKeys)
        {
            if (!IsRegistered(key))
                throw new UnknownPermissionException(key);
        }
    }
}
