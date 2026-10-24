namespace LowlandTech.Core.Plugins;

/// <summary>
/// Identity of a single node-type or edge-type descriptor contributed by a plugin.
/// Carries enough information for the frontend to register the descriptor in its
/// manifest without scanning the plugin's assembly.
/// </summary>
public sealed record PluginGraphDescriptorEntry(
    Guid DescriptorId,
    string DescriptorKey,
    string DescriptorLabel,
    PluginGraphDescriptorKind Kind);

public enum PluginGraphDescriptorKind
{
    NodeType,
    EdgeType
}

/// <summary>
/// Describes the graph descriptors (node types and edge types) that a plugin owns.
/// The frontend uses this manifest to register plugin-owned descriptors without
/// scanning the entire plugin catalogue. The backend receives the same manifest
/// for descriptor identity/version tracking.
/// </summary>
public sealed record PluginGraphManifest(
    string PluginId,
    string? Version,
    IReadOnlyList<PluginGraphDescriptorEntry> NodeDescriptors,
    IReadOnlyList<PluginGraphDescriptorEntry> EdgeDescriptors);
