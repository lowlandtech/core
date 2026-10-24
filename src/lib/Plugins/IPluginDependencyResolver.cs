namespace LowlandTech.Core.Plugins;

/// <summary>
/// Topologically sorts plugins by their declared <see cref="PluginDescriptor.DependsOn"/>
/// and validates each plugin's dependencies are present and version-compatible.
/// SPEC2050 US14.
/// </summary>
public interface IPluginDependencyResolver
{
    /// <summary>
    /// Returns the input plugins ordered so that every dependency precedes its dependents.
    /// </summary>
    /// <exception cref="PluginDependencyMissingException">A plugin declares a dependency on an id not in the input.</exception>
    /// <exception cref="PluginVersionIncompatibleException">A dependency's installed version falls outside the required range.</exception>
    /// <exception cref="PluginDependencyCycleException">The dependency graph contains a cycle.</exception>
    IReadOnlyList<PluginDescriptor> SortByLoadOrder(IEnumerable<PluginDescriptor> plugins);

    /// <summary>
    /// Validates a single plugin's dependencies against an available set without sorting.
    /// </summary>
    void ValidateDependencies(PluginDescriptor plugin, IEnumerable<PluginDescriptor> available);

    /// <summary>
    /// Returns the ids of plugins in <paramref name="available"/> that depend (directly) on <paramref name="pluginId"/>.
    /// </summary>
    IReadOnlyList<string> FindDependents(string pluginId, IEnumerable<PluginDescriptor> available);
}
