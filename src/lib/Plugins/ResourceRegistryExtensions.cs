namespace LowlandTech.Core.Plugins;

/// <summary>
/// Convenience overloads for <see cref="IResourceRegistry"/> used in generated plugin packages.
/// </summary>
public static class ResourceRegistryExtensions
{
    /// <summary>Register an i18n namespace contributed by a plugin.</summary>
    public static void Register(this IResourceRegistry registry, string @namespace, string pluginId)
        => registry.RegisterNamespace(@namespace, pluginId);
}
