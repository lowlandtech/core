namespace LowlandTech.Core.Graph.NodeTypes;

/// <summary>
/// Typed header descriptor for the SessionBlock settings area.
/// </summary>
public class SettingsHeaderNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.SettingsHeader.Id;

    public Guid Id => NodeTypeId;
    public string Key => "settings";
    public string Label => "Settings";
    public string? Icon => nameof(Icons.Settings);
    public string? BlockComponent => null;
    public int SortOrder => 7;
    public Type? EntityType => null;
    public bool IsHeader => true;
}
