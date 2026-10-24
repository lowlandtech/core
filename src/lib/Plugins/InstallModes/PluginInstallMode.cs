namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Extension mechanism by which a plugin package is delivered and activated.
/// SPEC2050 US20 — coexisting install modes.
/// </summary>
public enum PluginInstallMode
{
    /// <summary>PR → image build → compose route swap. Primary for hosted web.</summary>
    Routed,

    /// <summary>AssemblyLoadContext swap in-process. Primary for desktop; optional for web.</summary>
    HotReload,

    /// <summary>External container + iframe proxy. For federated external services.</summary>
    Sidecar,
}
