namespace LowlandTech.Core.Graph.NodeTypes;

public class ScenarioNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.Scenario.Id;

    public Guid Id => NodeTypeId;
    public string Key => "scenario";
    public string Label => "Scenario";
    public string? Icon => nameof(Icons.Scenario);
    public string? BlockComponent => "ScenarioBlock";
    public int SortOrder => 202;
    public Type? EntityType => null;
    public bool IsListable => true;
    public IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders =>
    [
        new("Steps", "Steps", "scenario-step", 0, nameof(Icons.Step),
            ParentToHeaderEdgeKey: "has.steps", HeaderToChildEdgeKey: "has.step"),
    ];
}
