namespace LowlandTech.Core.Plugins;

/// <summary>
/// Permission key declared by a plugin or feature, discoverable by the host RBAC admin UI.
/// SPEC2050 US09.
/// </summary>
public sealed record PermissionDescriptor(
    string Key,
    string PluginId,
    string? FeatureId,
    string? DisplayName,
    string? Description);
