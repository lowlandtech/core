namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Descriptor for a <c>list</c> node: a user-created collection that can
/// reference any listable node type through an <c>allows</c> relationship.
/// </summary>
public class ListNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.List.Id;

    public Guid Id => NodeTypeId;
    public string Key => "list";
    public string Label => "List";
    public string? Icon => nameof(Icons.List);
    public string? BlockComponent => "NodeBlock";
    public int SortOrder => 6;
    public Type? EntityType => null;
}
