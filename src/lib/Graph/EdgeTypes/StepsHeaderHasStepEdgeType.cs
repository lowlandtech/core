namespace LowlandTech.Core.Graph.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>StepsHeader → Step</c> create-relationship.
/// verb="create": when the Steps header inside a Flow is selected in the dashboard
/// tree, <c>ResolveHeaderEdgeTypes</c> resolves this edge via label-match on
/// <see cref="StepsHeaderNodeType.Label"/> and the session command bar presents
/// "Add Step" as the primary create intent.
/// </summary>
public class StepsHeaderHasStepEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.StepsHeaderHasStep.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "step.header.has.step";
    public string  Label            => "Create Step";
    public Guid    SourceNodeTypeId => StepsHeaderNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => StepNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "StepCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Step",
    };
}
