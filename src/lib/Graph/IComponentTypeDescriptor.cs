namespace LowlandTech.Core.Graph;

/// <summary>
/// Declares a frontend component type as a graph NodeType.
/// Implementations are static descriptors that live in the app project.
/// Each component type gets a stable Guid and is registered as a NodeType in the graph.
///
/// The descriptor maps a NodeType key to a Blazor component Type,
/// so the graph can resolve what to render for a given node.
/// </summary>
public interface IComponentTypeDescriptor
{
    /// <summary>
    /// Stable NodeType identifier. Must match the corresponding INodeTypeDescriptor.Id
    /// when this component renders an existing NodeType, or be a new unique Guid
    /// for component-only NodeTypes.
    /// </summary>
    static abstract Guid NodeTypeId { get; }

    /// <summary>
    /// The NodeType key this component renders (e.g. "workflow", "branch", "terminal").
    /// </summary>
    static abstract string NodeTypeKey { get; }

    /// <summary>
    /// The Blazor component Type to render.
    /// </summary>
    static abstract Type ComponentType { get; }
}
