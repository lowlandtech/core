namespace LowlandTech.Core.Plugins;

/// <summary>
/// Resolves a live DbContext instance for a given plugin id at runtime.
/// SPEC2050 US02.
/// </summary>
/// <remarks>
/// Implementations typically delegate to <c>IServiceProvider</c> using the
/// <see cref="PluginContextDescriptor.ContextType"/> recorded in the registry.
/// </remarks>
public interface IPluginContextFactory
{
    /// <summary>
    /// Resolve the DbContext registered for <paramref name="pluginId"/> from the current
    /// service scope.
    /// </summary>
    /// <exception cref="UnknownPluginException">No plugin context is registered for the id.</exception>
    object Resolve(string pluginId);
}
