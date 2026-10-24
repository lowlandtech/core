namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Identifier for a plugin package to be installed.
/// SPEC2050 US20.
/// </summary>
public sealed record PluginRef(string PluginId, string Version, string Source);
