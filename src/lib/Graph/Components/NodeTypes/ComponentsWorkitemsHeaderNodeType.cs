namespace LowlandTech.Core.Graph.Components.NodeTypes;

/// <summary>
/// NodeType descriptor for the <c>Workitems</c> header in the user-context tree.
/// Headers group entities under a domain label without backing any entity row.
/// <c>IsHeader = true</c> lets the dashboard registry resolve the correct
/// <see cref="LowlandTech.Core.Graph.Components.EdgeTypes.ComponentsWorkitemsHeaderHasWorkitemEdgeType"/>
/// (verb="create") so the session command bar shows "Add Workitem" when this header
/// is selected.
/// </summary>
public class ComponentsWorkitemsHeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.ComponentsWorkitemsHeader.Id;

    public Guid    Id             => NodeTypeId;
    public string  Key            => "workitems.header";
    public string  Label          => "Workitems";
    public string? Icon           => nameof(Icons.Workitem);
    public string? BlockComponent => null;
    public int     SortOrder      => 0;
    public Type?   EntityType     => null;
    public bool    IsHeader       => true;
}
