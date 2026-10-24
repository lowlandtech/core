namespace LowlandTech.Core.Plugins;

/// <summary>
/// Outcome of <see cref="IPluginHostService.InstallAsync"/>.
/// SPEC2050 US08.
/// </summary>
public sealed record PluginInstallResult(
    bool Success,
    string PluginId,
    string Version,
    string? FailureReason = null,
    bool AlreadyInstalled = false);

/// <summary>
/// Outcome of <see cref="IPluginHostService.UninstallAsync"/>.
/// SPEC2050 US08.
/// </summary>
public sealed record PluginUninstallResult(
    bool Success,
    string PluginId,
    string? FailureReason = null,
    IReadOnlyList<string>? BlockingDependents = null);
