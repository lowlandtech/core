namespace LowlandTech.Core.Graph.Components.NodeTypes;

/// <summary>
/// NodeType descriptor for a <c>workitem</c> node in the user-context tree.
/// </summary>
public class ComponentsWorkitemNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.ComponentsWorkitem.Id;

    public Guid    Id             => NodeTypeId;
    public string  Key            => "workitem";
    public string  Label          => "Workitem";
    public string? Icon           => nameof(Icons.Workitem);
    public string? BlockComponent => "NodeBlock";
    public int     SortOrder      => 0;
    public Type?   EntityType     => null;
}
