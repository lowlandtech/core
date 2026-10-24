
namespace LowlandTech.Core.Graph.EdgeTypes;

public class HasSettingsEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.HasSettings.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "has.settings";
    public string Label => "Has Settings";
    public Guid SourceNodeTypeId => UserContextNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => SettingsHeaderNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "false",
        ["label"] = "Settings",
        ["icon"] = nameof(Icons.Settings),
        ["cascadeDelete"] = "true",
    };
}
