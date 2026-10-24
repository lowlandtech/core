namespace LowlandTech.Core.Graph.NodeTypes;

public class ProjectNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Project.Id;

    public Guid Id => NodeTypeId;
    public string Key => "project";
    public string Label => "Project";
    public string? Icon => nameof(Icons.Briefcase);
    public string? BlockComponent => null;
    public int SortOrder => 17;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
