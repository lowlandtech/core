namespace LowlandTech.Core.Graph.NodeTypes;

public class ScenarioStepNodeType : INodeTypeDescriptor
{
    public static readonly Guid NodeTypeId = GraphConstants.NodeTypes.ScenarioStep.Id;

    public Guid Id => NodeTypeId;
    public string Key => "scenario-step";
    public string Label => "Scenario Step";
    public string? Icon => nameof(Icons.Step);
    public string? BlockComponent => "ScenarioStepBlock";
    public int SortOrder => 206;
    public Type? EntityType => null;
    public bool IsListable => true;
    public bool IsLeaf => true;
}
