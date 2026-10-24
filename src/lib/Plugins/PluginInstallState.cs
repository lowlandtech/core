namespace LowlandTech.Core.Plugins;

/// <summary>
/// Lifecycle state of a plugin known to <see cref="IPluginHostService"/>.
/// SPEC2050 US08.
/// </summary>
public enum PluginInstallState
{
    /// <summary>Listed in a feed but not installed locally.</summary>
    Available = 0,

    /// <summary>Installed and currently running.</summary>
    Installed = 1,

    /// <summary>Installed but suspended (background jobs stopped, awaiting hot-reload swap).</summary>
    Suspended = 2,

    /// <summary>Install was attempted but failed.</summary>
    Failed = 3,
}
