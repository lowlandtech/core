namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the SessionBlock settings outbox projection.
/// </summary>
public class SettingsHasOutboxEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.SettingsHasOutbox.Id;
    public Guid Id => EdgeTypeId;
    public string Key => "settings.has.outbox";
    public string Label => "Has Outbox";
    public Guid SourceNodeTypeId => SettingsHeaderNodeType.NodeTypeId;
    public Guid TargetNodeTypeId => SessionOutboxNodeType.NodeTypeId;
    public string? ClrType => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["required"] = "true",
        ["label"] = "Outbox",
        ["icon"] = nameof(Icons.Inbox),
    };
}
