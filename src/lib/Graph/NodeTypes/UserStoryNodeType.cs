namespace LowlandTech.Core.Graph.NodeTypes;

public class UserStoryNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.UserStory.Id;

    public Guid Id => NodeTypeId;
    public string Key => "user-story";
    public string Label => "User Story";
    public string? Icon => nameof(Icons.UserStory);
    public string? BlockComponent => "UserStoryBlock";
    public int SortOrder => 201;
    public Type? EntityType => null;
    public bool IsListable => true;
}
