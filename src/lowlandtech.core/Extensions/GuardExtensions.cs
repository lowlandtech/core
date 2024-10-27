namespace LowlandTech.Core.Extensions;

/// <summary>
/// Guard extensions.
/// </summary>
public static class GuardExtensions
{
    /// <summary>
    /// Guard against missing plugin id.
    /// </summary>
    /// <param name="guardClause">The guard close.</param>
    /// <param name="plugin">The plugin.</param>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="message">The message</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string MissingPluginId(this IGuardClause guardClause, IPlugin plugin, string parameterName, string message = $"Invalid plugin id, it must be provided")
    {
        Guard.Against.Null(plugin, nameof(plugin));

        if (Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginId)) is not PluginId pluginId)
        {
            throw new ArgumentException(message, parameterName);
        }

        return pluginId.Id;
    }
}
