namespace LowlandTech.Core.Graph.NodeTypes;

public class FeatureNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Feature.Id;

    public Guid Id => NodeTypeId;
    public string Key => "feature";
    public string Label => "Feature";
    public string? Icon => nameof(Icons.File);
    public string? BlockComponent => "FeatureBlock";
    public int SortOrder => 200;
    public Type? EntityType => null;
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new("Scenarios", "Scenarios", "scenario", 0, nameof(Icons.Scenario),
            ParentToHeaderEdgeKey: "has.scenarios", HeaderToChildEdgeKey: "has.scenario"),
        new("Actions", "Actions", "action", 1, nameof(Icons.Run),
            ParentToHeaderEdgeKey: "has.actions", HeaderToChildEdgeKey: "has.action"),
        new("Events", "Events", "event", 2, nameof(Icons.Bell),
            ParentToHeaderEdgeKey: "has.events", HeaderToChildEdgeKey: "has.event"),
        new("Handlers", "Handlers", "handler", 3, nameof(Icons.ServiceProcessor),
            ParentToHeaderEdgeKey: "has.handlers", HeaderToChildEdgeKey: "has.handler"),
    ];
}
