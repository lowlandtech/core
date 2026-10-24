namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default implementation of <see cref="IPluginDependencyResolver"/>.
/// Uses Kahn's algorithm for topological sort and a depth-first walk for cycle detection.
/// </summary>
public sealed class PluginDependencyResolver : IPluginDependencyResolver
{
    public IReadOnlyList<PluginDescriptor> SortByLoadOrder(IEnumerable<PluginDescriptor> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var byId = plugins.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

        // Validate every dependency exists and is version-compatible.
        foreach (var p in byId.Values)
            ValidateDependencies(p, byId.Values);

        // Detect cycles via DFS first so the topological sort can assume DAG.
        DetectCycles(byId);

        // Kahn's algorithm.
        var inDegree = byId.Values.ToDictionary(
            p => p.Id,
            p => p.DependsOn.Count(d => byId.ContainsKey(d.PluginId)),
            StringComparer.OrdinalIgnoreCase);

        // Adjacency: dependency-id -> list of plugin-ids that depend on it.
        var adjacency = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in byId.Values)
        foreach (var dep in p.DependsOn)
        {
            if (!byId.ContainsKey(dep.PluginId)) continue;
            if (!adjacency.TryGetValue(dep.PluginId, out var list))
                adjacency[dep.PluginId] = list = [];
            list.Add(p.Id);
        }

        // Stable order: process zero-in-degree plugins alphabetically for deterministic output.
        var ready = new SortedSet<string>(
            inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key),
            StringComparer.OrdinalIgnoreCase);

        var ordered = new List<PluginDescriptor>(byId.Count);
        while (ready.Count > 0)
        {
            var current = ready.Min!;
            ready.Remove(current);
            ordered.Add(byId[current]);

            if (!adjacency.TryGetValue(current, out var dependents)) continue;
            foreach (var dependent in dependents)
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                    ready.Add(dependent);
            }
        }

        // Should never happen because cycles were already detected.
        if (ordered.Count != byId.Count)
            throw new PluginDependencyCycleException(byId.Keys.Except(ordered.Select(o => o.Id)).ToArray());

        return ordered;
    }

    public void ValidateDependencies(PluginDescriptor plugin, IEnumerable<PluginDescriptor> available)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(available);

        var byId = available.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var dep in plugin.DependsOn)
        {
            if (!byId.TryGetValue(dep.PluginId, out var dependency))
                throw new PluginDependencyMissingException(plugin.Id, dep.PluginId);

            if (!IsVersionInRange(dependency.Version, dep.MinVersion, dep.MaxVersion))
                throw new PluginVersionIncompatibleException(
                    plugin.Id, dep.PluginId, dep.MinVersion, dep.MaxVersion, dependency.Version);
        }
    }

    public IReadOnlyList<string> FindDependents(string pluginId, IEnumerable<PluginDescriptor> available)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        ArgumentNullException.ThrowIfNull(available);

        return available
            .Where(p => p.DependsOn.Any(d => string.Equals(d.PluginId, pluginId, StringComparison.OrdinalIgnoreCase)))
            .Select(p => p.Id)
            .ToArray();
    }

    // DFS coloring: 0=White (unvisited), 1=Gray (on current stack), 2=Black (done)
    private const int White = 0;
    private const int Gray = 1;
    private const int Black = 2;

    private static void DetectCycles(Dictionary<string, PluginDescriptor> byId)
    {
        var color = byId.ToDictionary(kv => kv.Key, _ => White, StringComparer.OrdinalIgnoreCase);
        var path = new Stack<string>();

        foreach (var id in byId.Keys)
        {
            if (color[id] == White)
                Visit(id, byId, color, path);
        }
    }

    private static void Visit(
        string id,
        Dictionary<string, PluginDescriptor> byId,
        Dictionary<string, int> color,
        Stack<string> path)
    {
        color[id] = Gray;
        path.Push(id);

        foreach (var dep in byId[id].DependsOn)
        {
            if (!byId.ContainsKey(dep.PluginId)) continue;
            var depColor = color[dep.PluginId];
            if (depColor == Gray)
            {
                // Cycle: extract path from dep.PluginId back to current id, then close.
                var cycle = path.Reverse().SkipWhile(p => !string.Equals(p, dep.PluginId, StringComparison.OrdinalIgnoreCase)).ToList();
                cycle.Add(dep.PluginId);
                throw new PluginDependencyCycleException(cycle);
            }
            if (depColor == White)
                Visit(dep.PluginId, byId, color, path);
        }

        path.Pop();
        color[id] = Black;
    }

    private static bool IsVersionInRange(string installed, string min, string? max)
    {
        var installedVersion = ParseVersion(installed);
        var minVersion = ParseVersion(min);
        if (installedVersion < minVersion) return false;
        if (max is not null && installedVersion > ParseVersion(max)) return false;
        return true;
    }

    private static Version ParseVersion(string raw)
    {
        // Strip any pre-release suffix ("1.0.0-alpha" -> "1.0.0") for ordering purposes.
        var dash = raw.IndexOf('-');
        var stripped = dash >= 0 ? raw[..dash] : raw;
        return Version.TryParse(stripped, out var v)
            ? v
            : throw new FormatException($"Invalid plugin version '{raw}'.");
    }
}
