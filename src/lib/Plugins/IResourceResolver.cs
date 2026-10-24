namespace LowlandTech.Core.Plugins;

/// <summary>
/// Resolves a localised string for a registered resource key. Locale precedence:
/// host overrides → plugin default locale → fallback locale (typically <c>"en"</c>).
/// SPEC2050 US09.
/// </summary>
public interface IResourceResolver
{
    /// <summary>Look up a resource string for the given culture, applying fallback rules.</summary>
    string? Resolve(string key, string culture);

    /// <summary>Register a translation for a key in a specific culture inside the plugin's default catalog.</summary>
    void AddPluginTranslation(string key, string culture, string value);

    /// <summary>Register a host-level override that wins over plugin-supplied translations.</summary>
    void AddHostOverride(string key, string culture, string value);
}

public sealed class ResourceResolver(string fallbackCulture = "en") : IResourceResolver
{
    private readonly Dictionary<(string Culture, string Key), string> _pluginCatalog = new();
    private readonly Dictionary<(string Culture, string Key), string> _hostOverrides = new();
    private readonly object _lock = new();

    public string FallbackCulture { get; } = fallbackCulture;

    public string? Resolve(string key, string culture)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(culture);

        lock (_lock)
        {
            // 1. Host override in requested culture
            if (_hostOverrides.TryGetValue((culture, key), out var hostExact)) return hostExact;

            // 2. Plugin translation in requested culture
            if (_pluginCatalog.TryGetValue((culture, key), out var pluginExact)) return pluginExact;

            // 3. Host override in fallback culture
            if (_hostOverrides.TryGetValue((FallbackCulture, key), out var hostFallback)) return hostFallback;

            // 4. Plugin translation in fallback culture
            if (_pluginCatalog.TryGetValue((FallbackCulture, key), out var pluginFallback)) return pluginFallback;

            return null;
        }
    }

    public void AddPluginTranslation(string key, string culture, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(value);
        lock (_lock) _pluginCatalog[(culture, key)] = value;
    }

    public void AddHostOverride(string key, string culture, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(value);
        lock (_lock) _hostOverrides[(culture, key)] = value;
    }
}
