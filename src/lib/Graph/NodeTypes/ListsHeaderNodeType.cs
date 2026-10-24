namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Typed header descriptor for the <c>Lists</c> navigation group under a UserContext.
/// </summary>
public class ListsHeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.ListsHeader.Id;

    public Guid Id => NodeTypeId;
    public string Key => "lists";
    public string Label => "Lists";
    public string? Icon => nameof(Icons.List);
    public string? BlockComponent => null;
    public int SortOrder => 5;
    public Type? EntityType => null;
    public bool IsHeader => true;
}
