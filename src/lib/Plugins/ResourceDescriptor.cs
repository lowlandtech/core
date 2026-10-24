namespace LowlandTech.Core.Plugins;

/// <summary>
/// i18n resource key declared by a plugin or feature.
/// SPEC2050 US09.
/// </summary>
public sealed record ResourceDescriptor(
    string Key,
    string Namespace,
    string PluginId,
    string? FeatureId);
