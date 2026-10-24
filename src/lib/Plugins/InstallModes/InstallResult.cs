namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Terminal outcome of an <see cref="IPluginInstaller.ExecuteAsync"/> call.
/// Mode-specific fields are populated only by the installer for that mode.
/// SPEC2050 US20.
/// </summary>
public sealed record InstallResult(
    bool Succeeded,
    string? PullRequestUrl = null,
    string? DeployedImageTag = null,
    string? NewAlcName = null,
    string? SidecarContainerId = null,
    string? Error = null);
