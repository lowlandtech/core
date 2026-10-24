namespace LowlandTech.Core.Plugins;

/// <summary>
/// Convenience overloads for <see cref="IPermissionRegistry"/> used in generated plugin packages.
/// </summary>
public static class PermissionRegistryExtensions
{
    /// <summary>Register a permission key with only a plugin id, leaving display name and description null.</summary>
    public static void Register(this IPermissionRegistry registry, string key, string pluginId)
        => registry.Register(new PermissionDescriptor(key, pluginId, null, null, null));
}
