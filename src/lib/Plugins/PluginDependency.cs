namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declares a runtime dependency from one plugin on another.
/// SemVer-style minimum (and optional maximum) version range.
/// </summary>
public sealed record PluginDependency(
    string PluginId,
    string MinVersion,
    string? MaxVersion = null);
