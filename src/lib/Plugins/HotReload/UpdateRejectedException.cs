namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Thrown when a version update request is rejected (e.g. downgrade blocked, pre-release filtered).
/// SPEC2050 US05 SC12.
/// </summary>
public sealed class UpdateRejectedException : Exception
{
    public string PluginId { get; }
    public string Reason { get; }

    public UpdateRejectedException(string pluginId, string reason)
        : base($"Update rejected for plugin '{pluginId}': {reason}")
    {
        PluginId = pluginId;
        Reason = reason;
    }
}
