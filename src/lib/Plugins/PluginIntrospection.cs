namespace LowlandTech.Core.Plugins;

/// <summary>
/// Snapshot of every contribution made by a single plugin to the host registries.
/// SPEC2050 US08.
/// </summary>
public sealed record PluginIntrospection(
    string PluginId,
    string Version,
    PluginInstallState State,
    IReadOnlyList<FeatureDescriptor> Features,
    IReadOnlyList<PageTypeDescriptor> Pages,
    IReadOnlyList<BlockTypeDescriptor> Blocks,
    IReadOnlyList<BackgroundJobDescriptor> Jobs,
    IReadOnlyList<McpToolDescriptor> McpTools,
    IReadOnlyList<EdgeTypeDescriptor> EdgeTypes,
    IReadOnlyList<PermissionDescriptor> Permissions,
    IReadOnlyList<ResourceDescriptor> Resources,
    PluginContextDescriptor? Context);

/// <summary>
/// Walks the live DRD registries and produces a <see cref="PluginIntrospection"/> for a
/// single plugin id. The HTTP endpoint that exposes this lives in the API layer; this
/// service is the in-process aggregator.
/// SPEC2050 US08.
/// </summary>
public interface IPluginIntrospectionService
{
    PluginIntrospection? Introspect(string pluginId);
}

public sealed class PluginIntrospectionService(
    IPluginRegistry plugins,
    IFeatureRegistry features,
    IPageTypeRegistry pages,
    IBlockTypeRegistry blocks,
    IJobRegistry jobs,
    IMcpToolRegistry mcpTools,
    IPluginEdgeTypeRegistry edgeTypes,
    IPermissionRegistry permissions,
    IResourceRegistry resources,
    IPluginContextRegistry contexts,
    PluginHostState hostState) : IPluginIntrospectionService
{
    public PluginIntrospection? Introspect(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        var descriptor = plugins.Find(pluginId);
        if (descriptor is null) return null;

        var pluginFeatures = features.All
            .Where(f => Eq(f.PluginId, pluginId))
            .ToArray();
        var featureIds = pluginFeatures.Select(f => f.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new PluginIntrospection(
            PluginId: descriptor.Id,
            Version: descriptor.Version,
            State: hostState.Get(descriptor.Id, PluginInstallState.Installed),
            Features: pluginFeatures,
            Pages: pages.All.Where(p => featureIds.Contains(p.FeatureId)).ToArray(),
            Blocks: blocks.All.Where(b => featureIds.Contains(b.FeatureId)).ToArray(),
            Jobs: jobs.All.Where(j => featureIds.Contains(j.FeatureId)).ToArray(),
            McpTools: mcpTools.All.Where(t => featureIds.Contains(t.FeatureId)).ToArray(),
            EdgeTypes: edgeTypes.All.Where(e => featureIds.Contains(e.FeatureId)).ToArray(),
            Permissions: permissions.All.Where(p => Eq(p.PluginId, pluginId)).ToArray(),
            Resources: resources.All.Where(r => Eq(r.PluginId, pluginId)).ToArray(),
            Context: contexts.FindByPluginId(pluginId));
    }

    private static bool Eq(string? a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
