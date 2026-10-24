namespace LowlandTech.Core.Plugins;

/// <summary>
/// Emitted by <c>RoutedInstaller</c> immediately after the PR is opened,
/// signalling that the install pipeline has started and phase = awaiting-build.
/// SPEC2050 US20 SC01 UAC01 / UAC03.
/// </summary>
public sealed record InstallStarted(string PluginId, string Version, string PullRequestUrl);

/// <summary>
/// Emitted by <c>RoutedInstaller</c> after compose traffic is switched to the new image.
/// SPEC2050 US20 SC01 UAC07.
/// </summary>
public sealed record RoutedInstalled(string PluginId, string Version, string DeployedImageTag);

/// <summary>
/// Emitted by <c>HotReloadInstaller</c> after the ALC swap completes successfully.
/// SPEC2050 US20 SC02 UAC06.
/// </summary>
public sealed record HotReloadInstalled(string PluginId, string OldAlcName, string NewAlcName);

/// <summary>
/// Emitted by <c>SidecarInstaller</c> after the sidecar container is running and
/// the iframe route is registered.
/// SPEC2050 US20 SC03 UAC05.
/// </summary>
public sealed record SidecarInstalled(string PluginId, string ContainerEndpoint);

/// <summary>
/// Emitted when any installer rolls back after a failure.
/// SPEC2050 US20 SC05 UAC04 / SC06 UAC04.
/// </summary>
public sealed record InstallRolledBack(string PluginId, string Reason);
