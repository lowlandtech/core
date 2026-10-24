namespace LowlandTech.Core.Graph.NodeTypes;

public class HeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Header.Id;

    public Guid Id => NodeTypeId;
    public string Key => "header";
    public string Label => "Header";
    public string? Icon => null;
    public string? BlockComponent => null;
    public int SortOrder => 1;
    public Type? EntityType => null;
}
