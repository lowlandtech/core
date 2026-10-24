namespace LowlandTech.Core.Graph.NodeTypes;

public class HandlerNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Handler.Id;

    public Guid Id => NodeTypeId;
    public string Key => "handler";
    public string Label => "Handler";
    public string? Icon => nameof(Icons.ServiceProcessor);
    public string? BlockComponent => "HandlerBlock";
    public int SortOrder => 205;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
