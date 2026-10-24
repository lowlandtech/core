namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Abstracts the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> operations needed
/// by <see cref="HotReloadService"/> so tests can substitute a fake without touching real
/// assembly loading.
/// SPEC2050 US05.
/// </summary>
public interface IAlcLoader
{
    /// <summary>
    /// Loads the plugin package from the given <paramref name="stagingPath"/> and returns
    /// a logical ALC name and the loaded <see cref="IApiPlugin"/>.
    /// </summary>
    Task<(string AlcName, IApiPlugin Package)> LoadAsync(string pluginId, string stagingPath, CancellationToken ct = default);

    /// <summary>Unloads the ALC identified by <paramref name="alcName"/>.</summary>
    void Unload(string alcName);
}
