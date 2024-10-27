namespace LowlandTech.Core.Types;

/// <summary>
/// Configuration for a plugin.
/// </summary>
public class PluginConfig
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the active state.
    /// </summary>
    public bool IsActive { get; set; }
}
