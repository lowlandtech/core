namespace LowlandTech.Core.Plugins;

/// <summary>
/// Persistent Layer 3 store for feature settings overrides. Values are stored as
/// JSON strings keyed by (featureId, key).
/// SPEC2050 US07.
/// </summary>
/// <remarks>
/// Production hosts back this with a database table; tests and local dev can use
/// <see cref="InMemoryFeatureSettingsStore"/>.
/// </remarks>
public interface IFeatureSettingsStore
{
    string? Get(string featureId, string key);
    void Set(string featureId, string key, string jsonValue);
    bool Clear(string featureId, string key);
}

/// <summary>
/// Default in-memory <see cref="IFeatureSettingsStore"/> useful for tests and local dev.
/// </summary>
public sealed class InMemoryFeatureSettingsStore : IFeatureSettingsStore
{
    private readonly Dictionary<(string FeatureId, string Key), string> _values = new();
    private readonly object _lock = new();

    public string? Get(string featureId, string key)
    {
        lock (_lock)
        {
            return _values.TryGetValue((featureId, key), out var v) ? v : null;
        }
    }

    public void Set(string featureId, string key, string jsonValue)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(jsonValue);
        lock (_lock) _values[(featureId, key)] = jsonValue;
    }

    public bool Clear(string featureId, string key)
    {
        lock (_lock) return _values.Remove((featureId, key));
    }
}
