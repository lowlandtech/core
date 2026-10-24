
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Optional marker implemented by an <see cref="IApiPlugin"/> whose entities
/// participate in the host graph layer. The host uses this to:
/// <list type="number">
///   <item>Compose the set of assemblies scanned by <c>INodeTypeRegistry</c>, so
///     plugin <c>INodeTypeDescriptor</c> / <c>IEdgeTypeDescriptor</c> types are
///     discovered alongside Core's built-in descriptors.</item>
///   <item>Attach a <c>PluginGraphSyncInterceptor</c> to each plugin-owned
///     <see cref="Microsoft.EntityFrameworkCore.DbContext"/> listed in
///     <see cref="SyncedDbContextTypes"/>, so entity Add/Update/Delete saves
///     mirror to graph nodes (and link tables to graph edges) in the host's
///     FoundryContext with the <c>node.Id == entity.Id</c> invariant intact.</item>
/// </list>
///
/// Plugins that do not own navigable entities (pure-service plugins, UI-only
/// plugins) do not need to implement this — omitting it is a signal that the
/// plugin has nothing to contribute to the graph and saves the host from
/// scanning its assemblies unnecessarily.
///
/// See <c>docs/pluginize/issues/plugin-graph-contributions.md</c> for the
/// motivating design discussion.
/// </summary>
public interface IGraphContributingPlugin
{
    /// <summary>
    /// Assemblies whose non-abstract types implementing
    /// <c>LowlandTech.Core.Graph.INodeTypeDescriptor</c> or
    /// <c>LowlandTech.Core.Plugins.EdgeTypeDescriptor</c> the host should
    /// scan in addition to Core's own descriptor assembly. The plugin is not
    /// constrained to a magic namespace — every type in each listed assembly
    /// is considered.
    /// </summary>
    IEnumerable<Assembly> GraphDescriptorAssemblies { get; }

    /// <summary>
    /// CLR types of <see cref="Microsoft.EntityFrameworkCore.DbContext"/>s the
    /// plugin owns. For each type the host resolves the registered
    /// <c>DbContextOptionsBuilder&lt;T&gt;</c> pipeline and appends the
    /// <c>PluginGraphSyncInterceptor</c>. Returning an empty sequence is
    /// valid — plugins that only contribute descriptors (no entity writes in
    /// their own context) need not wire any interceptor.
    /// </summary>
    IEnumerable<Type> SyncedDbContextTypes { get; }
}
