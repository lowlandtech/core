
namespace LowlandTech.Core.Graph.NodeTypes;

public class ToolCommandNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.ToolCommand.Id;

    public Guid Id => NodeTypeId;
    public string Key => "tool-command";
    public string Label => "Command";
    public string? Icon => nameof(Icons.Code);
    public string? BlockComponent => "ToolCommandBlock";
    public int SortOrder => 16;
    public Type? EntityType => typeof(ToolCommand);
    public bool IsListable => true;

    public EntityParentRef ExtractParent(object entity)
        => entity is ToolCommand command ? new EntityParentRef("tool", command.ToolId) : EntityParentRef.None;
}
