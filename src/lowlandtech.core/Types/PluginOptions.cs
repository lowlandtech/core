namespace LowlandTech.Core.Types;

/// <summary>
/// Plugin options.
/// </summary>
public class PluginOptions
{
    /// <summary>
    /// Configuration name.
    /// </summary>
    public const string Name = "Plugins";

    /// <summary>
    /// Sets the plugins.
    /// </summary>
    public List<PluginConfig> Plugins { get; set; } = [];
}