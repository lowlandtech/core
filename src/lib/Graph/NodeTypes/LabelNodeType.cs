namespace LowlandTech.Core.Graph.NodeTypes;

public class LabelNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Label.Id;

    public Guid Id => NodeTypeId;
    public string Key => "label";
    public string Label => "Label";
    public string? Icon => nameof(Icons.Hash);
    public string? BlockComponent => null;
    public int SortOrder => 15;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
