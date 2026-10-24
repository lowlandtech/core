namespace LowlandTech.Core.Plugins;

/// <summary>
/// Maps a plugin's UI path to the running sidecar container's endpoint so the
/// main Blazor host can embed the plugin's UI via an iframe proxy.
/// SPEC2050 US20 SC03 UAC02.
/// </summary>
public interface IIframeRouteRegistry
{
    /// <summary>Register a new iframe route for the given plugin.</summary>
    void Register(string pluginId, string uiPath, string containerEndpoint);

    /// <summary>All currently-registered iframe routes.</summary>
    IReadOnlyList<IframeRoute> All { get; }

    /// <summary>Remove the iframe route for the given plugin (e.g. on uninstall).</summary>
    void Unregister(string pluginId);
}

/// <summary>
/// A single registered iframe route entry.
/// SPEC2050 US20 SC03 UAC02.
/// </summary>
public sealed record IframeRoute(string PluginId, string UiPath, string ContainerEndpoint);

/// <summary>
/// Default in-memory implementation of <see cref="IIframeRouteRegistry"/>.
/// Thread-safe for concurrent registrations.
/// SPEC2050 US20 SC03.
/// </summary>
public sealed class InMemoryIframeRouteRegistry : IIframeRouteRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<string, IframeRoute> _routes = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public void Register(string pluginId, string uiPath, string containerEndpoint)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        ArgumentNullException.ThrowIfNull(uiPath);
        ArgumentNullException.ThrowIfNull(containerEndpoint);

        lock (_lock)
            _routes[pluginId] = new IframeRoute(pluginId, uiPath, containerEndpoint);
    }

    /// <inheritdoc/>
    public IReadOnlyList<IframeRoute> All
    {
        get { lock (_lock) return _routes.Values.ToArray(); }
    }

    /// <inheritdoc/>
    public void Unregister(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        lock (_lock) _routes.Remove(pluginId);
    }
}
