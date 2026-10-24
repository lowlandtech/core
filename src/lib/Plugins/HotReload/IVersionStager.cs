namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Downloads and extracts a plugin version to a staging directory.
/// SPEC2050 US05 SC04.
/// </summary>
public interface IVersionStager
{
    /// <summary>
    /// Downloads the <paramref name="version"/> of <paramref name="pluginId"/> from the feed
    /// and extracts it to a subdirectory under the configured staging root.
    /// </summary>
    /// <returns>The full path to the extracted staging directory.</returns>
    Task<string> StageAsync(string pluginId, string version, CancellationToken ct = default);
}
