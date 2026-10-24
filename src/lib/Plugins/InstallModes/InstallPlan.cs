namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Ordered sequence of steps an <see cref="IPluginInstaller"/> intends to execute.
/// SPEC2050 US20.
/// </summary>
public sealed record InstallPlan(
    PluginInstallMode Mode,
    PluginRef Package,
    IReadOnlyList<InstallStep> Steps);
