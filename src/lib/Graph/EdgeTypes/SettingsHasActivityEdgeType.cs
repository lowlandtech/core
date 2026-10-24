namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the SessionBlock settings activity projection.
/// </summary>
public class SettingsHasActivityEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.SettingsHasActivity.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "settings.has.activity";
    public string Label => "Has Activity";
    public Guid SourceNodeTypeId => SettingsHeaderNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => SessionActivityNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Activity",
        ["icon"] = nameof(Icons.Clock),
    };
}
