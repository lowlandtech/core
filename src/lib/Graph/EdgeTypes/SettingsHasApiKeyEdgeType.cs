namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for creating API keys from SessionBlock settings.
/// </summary>
public class SettingsHasApiKeyEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.SettingsHasApiKey.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "settings.has.api-key";
    public string Label => "Create API Key";
    public Guid SourceNodeTypeId => SettingsHeaderNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => SessionApiKeyNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"] = "create",
        ["cardinality"] = "many",
        ["direction"] = "outgoing",
        ["ui.surface"] = "settings",
        ["ui.label"] = "Create API Key",
    };
}
