namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// NodeType descriptor for the <c>Flows</c> header in the user-context tree.
/// Headers group entities under a domain label without backing any entity row.
/// <c>IsHeader = true</c> lets the dashboard registry resolve the correct
/// <see cref="LowlandTech.Core.Graph.EdgeTypes.FlowsHeaderHasFlowEdgeType"/>
/// (verb="create") so the session command bar shows "Add Flow" when this header
/// is selected.
/// </summary>
public class FlowsHeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.FlowsHeader.Id;

    public Guid    Id             => NodeTypeId;
    public string  Key            => "flow.header";
    public string  Label          => "Flows";
    public string? Icon           => nameof(Icons.Workflows);
    public string? BlockComponent => null;
    public int     SortOrder      => 0;
    public Type?   EntityType     => null;
    public bool    IsHeader       => true;
}
