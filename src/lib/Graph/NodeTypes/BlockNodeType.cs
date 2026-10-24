namespace LowlandTech.Core.Graph.NodeTypes;

public class BlockNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Block.Id;

    public Guid Id => NodeTypeId;
    public string Key => "block";
    public string Label => "Block";
    public string? Icon => null;
    public string? BlockComponent => null;
    public int SortOrder => 11;
    public Type? EntityType => null;
    public bool IsLeaf => true;
}
