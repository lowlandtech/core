namespace LowlandTech.Core.Graph.NodeTypes;

public class PipelineNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Pipeline.Id;

    public Guid Id => NodeTypeId;
    public string Key => "pipeline";
    public string Label => "Pipeline";
    public string? Icon => nameof(Icons.Pipeline);
    public string? BlockComponent => null;
    public int SortOrder => 18;
    public Type? EntityType => null;
    public bool IsListable => true;
}
