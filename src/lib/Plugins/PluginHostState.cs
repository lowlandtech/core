namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks the per-plugin <see cref="PluginInstallState"/> known to the host.
/// SPEC2050 US08.
/// </summary>
public sealed class PluginHostState
{
    private readonly Dictionary<string, PluginInstallState> _state = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public PluginInstallState Get(string pluginId, PluginInstallState fallback = PluginInstallState.Available)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        lock (_lock) return _state.TryGetValue(pluginId, out var s) ? s : fallback;
    }

    public void Set(string pluginId, PluginInstallState state)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        lock (_lock) _state[pluginId] = state;
    }

    public bool Remove(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        lock (_lock) return _state.Remove(pluginId);
    }
}
