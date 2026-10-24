namespace LowlandTech.Core.Graph.Components.EdgeTypes;

/// <summary>
/// EdgeType descriptor for the <c>WorkitemsHeader → Workitem</c> create-relationship.
/// verb="create": when the Workitems header is selected in the dashboard tree the
/// session command bar auto-binds to this edge and presents "Add Workitem" as the
/// primary create intent.
/// </summary>
public class ComponentsWorkitemsHeaderHasWorkitemEdgeType : IEdgeTypeDescriptor
{
    public static readonly Guid EdgeTypeId = GraphConstants.EdgeTypes.ComponentsWorkitemsHeaderHasWorkitem.Id;
    public Guid Id => EdgeTypeId;
    public string  Key              => "workitems.header.has.workitem";
    public string  Label            => "Create Workitem";
    public Guid    SourceNodeTypeId => LowlandTech.Core.Graph.Components.NodeTypes.ComponentsWorkitemsHeaderNodeType.NodeTypeId;
    public Guid    TargetNodeTypeId => LowlandTech.Core.Graph.Components.NodeTypes.ComponentsWorkitemNodeType.NodeTypeId;
    public string? ClrType          => null;
    public IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>
    {
        ["verb"]        = "create",
        ["tool"]        = "WorkitemCreator",
        ["cardinality"] = "many",
        ["direction"]   = "outgoing",
        ["ui.surface"]  = "command-bar",
        ["ui.label"]    = "Create Workitem",
    };
}
