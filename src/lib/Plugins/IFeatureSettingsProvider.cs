
using System.Diagnostics.CodeAnalysis;

namespace LowlandTech.Core.Plugins;

/// <summary>
/// Three-layer settings provider for a feature: hardcoded defaults (Layer 1) →
/// IConfiguration section (Layer 2) → persistent DB overrides (Layer 3).
/// SPEC2050 US07.
/// </summary>
public interface IFeatureSettingsProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSettings>
    where TSettings : class, new()
{
    /// <summary>Resolve the merged settings for the bound feature.</summary>
    [RequiresUnreferencedCode(
        "Merging feature settings binds the configuration section onto the settings type by reflection, so the settings members cannot be statically analyzed.")]
    TSettings Get();

    /// <summary>Persist a Layer 3 override for a single property by name.</summary>
    [RequiresUnreferencedCode(
        "Persisting a feature setting serializes the override by reflection, so the value type cannot be statically analyzed.")]
    Task SetAsync(string key, object? value, CancellationToken cancellationToken = default);

    /// <summary>Remove a Layer 3 override and fall back to Layer 2 / Layer 1.</summary>
    Task ClearAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>Default <see cref="IFeatureSettingsProvider{TSettings}"/> with the three-layer merge.</summary>
public sealed class FeatureSettingsProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSettings>(
    string featureId,
    IConfiguration configuration,
    IFeatureSettingsStore store) : IFeatureSettingsProvider<TSettings>
    where TSettings : class, new()
{
    public string FeatureId { get; } = featureId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    [RequiresUnreferencedCode(
        "Merging feature settings binds the configuration section onto the settings type by reflection, so the settings members cannot be statically analyzed.")]
    public TSettings Get()
    {
        // Layer 1: hardcoded defaults from the type's parameterless constructor.
        var settings = new TSettings();

        // Layer 2: bind from configuration section "Features:{featureId}".
        var section = configuration.GetSection($"Features:{FeatureId}");
        if (section.Exists())
            section.Bind(settings);

        // Layer 3: apply DB overrides on top.
        foreach (var property in typeof(TSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite) continue;
            var raw = store.Get(FeatureId, property.Name);
            if (raw is null) continue;

            try
            {
                var value = JsonSerializer.Deserialize(raw, property.PropertyType, JsonOptions);
                property.SetValue(settings, value);
            }
            catch (JsonException)
            {
                // Malformed Layer 3 value: ignore and keep Layer 2 / Layer 1 value.
            }
        }

        return settings;
    }

    [RequiresUnreferencedCode(
        "Persisting a feature setting serializes the override by reflection, so the value type cannot be statically analyzed.")]
    public Task SetAsync(string key, object? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        var json = JsonSerializer.Serialize(value, JsonOptions);
        store.Set(FeatureId, key, json);
        return Task.CompletedTask;
    }

    public Task ClearAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        store.Clear(FeatureId, key);
        return Task.CompletedTask;
    }
}
