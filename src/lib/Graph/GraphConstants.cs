namespace LowlandTech.Core.Graph;

/// <summary>
/// A lightweight reference to a graph type that exposes both its stable
/// <see cref="Id"/> and human-readable <see cref="Name"/>.
/// </summary>
public readonly record struct GraphTypeRef(Guid Id, string Name);

/// <summary>
/// Central registry of well-known graph node and edge type identifiers.
/// Keeping every id in one place prevents accidental duplicate Guids across
/// <see cref="INodeTypeDescriptor"/> and <see cref="IEdgeTypeDescriptor"/> implementations.
/// </summary>
public static class GraphConstants
{
    public static class NodeTypes
    {
        public static readonly GraphTypeRef Action = new(new("a1000000-0000-0000-0000-000000000203"), "action");
        public static readonly GraphTypeRef Assignee = new(new("a1000000-0000-0000-0000-000000000103"), "assignee");
        public static readonly GraphTypeRef Block = new(new("a1000000-0000-0000-0000-000000000011"), "block");
        public static readonly GraphTypeRef Branch = new(new("a1000000-0000-0000-0000-000000000007"), "branch");
        public static readonly GraphTypeRef Column = new(new("a1000000-0000-0000-0000-000000000008"), "column");
        public static readonly GraphTypeRef ComponentsWorkitem = new(new("a1000000-0000-0000-0000-000000000046"), "workitem");
        public static readonly GraphTypeRef ComponentsWorkitemsHeader = new(new("a1000000-0000-0000-0000-000000000045"), "workitems.header");
        public static readonly GraphTypeRef Event = new(new("a1000000-0000-0000-0000-000000000204"), "event");
        public static readonly GraphTypeRef Feature = new(new("a1000000-0000-0000-0000-000000000200"), "feature");
        public static readonly GraphTypeRef Flow = new(new("a1000000-0000-0000-0000-000000000040"), "flow");
        public static readonly GraphTypeRef FlowsHeader = new(new("a1000000-0000-0000-0000-000000000042"), "flow.header");
        public static readonly GraphTypeRef Handler = new(new("a1000000-0000-0000-0000-000000000205"), "handler");
        public static readonly GraphTypeRef Header = new(new("a1000000-0000-0000-0000-000000000002"), "header");
        public static readonly GraphTypeRef Import = new(new("a1000000-0000-0000-0000-000000000024"), "import");
        public static readonly GraphTypeRef Label = new(new("a1000000-0000-0000-0000-000000000102"), "label");
        public static readonly GraphTypeRef List = new(new("a1000000-0000-0000-0000-000000000209"), "list");
        public static readonly GraphTypeRef ListsHeader = new(new("a1000000-0000-0000-0000-000000000208"), "lists");
        public static readonly GraphTypeRef Message = new(new("a1000000-0000-0000-0000-000000000010"), "message");
        public static readonly GraphTypeRef Package = new(new("a1000000-0000-0000-0000-000000000023"), "package");
        public static readonly GraphTypeRef PackagesHeader = new(new("a1000000-0000-0000-0000-000000000025"), "package.header");
        public static readonly GraphTypeRef PageItem = new(new("a1000000-0000-0000-0000-000000000051"), "page-item");
        public static readonly GraphTypeRef Page = new(new("a1000000-0000-0000-0000-000000000050"), "page");
        public static readonly GraphTypeRef Pipeline = new(new("a1000000-0000-0000-0000-000000000105"), "pipeline");
        public static readonly GraphTypeRef Project = new(new("a1000000-0000-0000-0000-000000000104"), "project");
        public static readonly GraphTypeRef Scenario = new(new("a1000000-0000-0000-0000-000000000202"), "scenario");
        public static readonly GraphTypeRef ScenarioStep = new(new("a1000000-0000-0000-0000-000000000206"), "scenario-step");
        public static readonly GraphTypeRef SessionActivity = new(new("a1000000-0000-0000-0000-000000000212"), "session-activity");
        public static readonly GraphTypeRef SessionApiKey = new(new("a1000000-0000-0000-0000-000000000211"), "session-api-key");
        public static readonly GraphTypeRef Session = new(new("a1000000-0000-0000-0000-000000000003"), "session");
        public static readonly GraphTypeRef SessionOutbox = new(new("a1000000-0000-0000-0000-000000000213"), "session-outbox");
        public static readonly GraphTypeRef SettingsHeader = new(new("a1000000-0000-0000-0000-000000000210"), "settings");
        public static readonly GraphTypeRef Step = new(new("a1000000-0000-0000-0000-000000000041"), "step");
        public static readonly GraphTypeRef StepsHeader = new(new("a1000000-0000-0000-0000-000000000043"), "step.header");
        public static readonly GraphTypeRef Table = new(new("a1000000-0000-0000-0000-000000000005"), "table");
        public static readonly GraphTypeRef Template = new(new("a1000000-0000-0000-0000-000000000020"), "template");
        public static readonly GraphTypeRef TemplatePackage = new(new("a1000000-0000-0000-0000-000000000022"), "template-package");
        public static readonly GraphTypeRef TemplateVersion = new(new("a1000000-0000-0000-0000-000000000021"), "template-version");
        public static readonly GraphTypeRef ToolCommand = new(new("a1000000-0000-0000-0000-000000000031"), "tool-command");
        public static readonly GraphTypeRef Tool = new(new("a1000000-0000-0000-0000-000000000030"), "tool");
        public static readonly GraphTypeRef UserContext = new(new("a1000000-0000-0000-0000-000000000001"), "user-context");
        public static readonly GraphTypeRef UserStory = new(new("a1000000-0000-0000-0000-000000000201"), "user-story");
        public static readonly GraphTypeRef Workflow = new(new("a1000000-0000-0000-0000-000000000006"), "workflow");
        public static readonly GraphTypeRef Workspace = new(new("a1000000-0000-0000-0000-000000000004"), "workspace");
        public static readonly GraphTypeRef WorkspacesHeader = new(new("a1000000-0000-0000-0000-000000000044"), "workspaces.header");
    }

    public static class EdgeTypes
    {
        public static readonly GraphTypeRef ComponentsWorkitemsHeaderHasWorkitem = new(new("b1000000-0000-0000-0000-000000000018"), "workitems.header.has.workitem");
        public static readonly GraphTypeRef ContainsBranch = new(new("b1000000-0000-0000-0000-000000000012"), "contains.branch");
        public static readonly GraphTypeRef ContainsColumn = new(new("b1000000-0000-0000-0000-000000000013"), "contains.column");
        public static readonly GraphTypeRef ContainsFeature = new(new("b1000000-0000-0000-0000-000000000202"), "contains.feature");
        public static readonly GraphTypeRef ContainsPageItem = new(new("b1000000-0000-0000-0000-000000000135"), "contains.page-item");
        public static readonly GraphTypeRef ContainsTable = new(new("b1000000-0000-0000-0000-000000000010"), "contains.table");
        public static readonly GraphTypeRef ContainsWorkflow = new(new("b1000000-0000-0000-0000-000000000011"), "contains.workflow");
        public static readonly GraphTypeRef FlowsHeaderHasFlow = new(new("b1000000-0000-0000-0000-000000000014"), "flow.header.has.flow");
        public static readonly GraphTypeRef HasAssignee = new(new("b1000000-0000-0000-0000-000000000104"), "has.assignee");
        public static readonly GraphTypeRef HasFeature = new(new("b1000000-0000-0000-0000-000000000201"), "has.feature");
        public static readonly GraphTypeRef HasFeatures = new(new("b1000000-0000-0000-0000-000000000200"), "has.features");
        public static readonly GraphTypeRef HasFlows = new(new("b1000000-0000-0000-0000-000000000006"), "has.flows");
        public static readonly GraphTypeRef HasLabel = new(new("b1000000-0000-0000-0000-000000000103"), "has.label");
        public static readonly GraphTypeRef HasList = new(new("b1000000-0000-0000-0000-000000000041"), "has.list");
        public static readonly GraphTypeRef HasLists = new(new("b1000000-0000-0000-0000-000000000040"), "has.lists");
        public static readonly GraphTypeRef HasMessage = new(new("b1000000-0000-0000-0000-000000000110"), "has.message");
        public static readonly GraphTypeRef HasPackages = new(new("b1000000-0000-0000-0000-000000000300"), "has.packages");
        public static readonly GraphTypeRef HasPage = new(new("b1000000-0000-0000-0000-000000000121"), "has.page");
        public static readonly GraphTypeRef HasPageItem = new(new("b1000000-0000-0000-0000-000000000131"), "has.page-item");
        public static readonly GraphTypeRef HasPageItems = new(new("b1000000-0000-0000-0000-000000000130"), "has.page-items");
        public static readonly GraphTypeRef HasPages = new(new("b1000000-0000-0000-0000-000000000120"), "has.pages");
        public static readonly GraphTypeRef HasPipeline = new(new("b1000000-0000-0000-0000-000000000108"), "has.pipeline");
        public static readonly GraphTypeRef HasProject = new(new("b1000000-0000-0000-0000-000000000105"), "has.project");
        public static readonly GraphTypeRef HasSessions = new(new("b1000000-0000-0000-0000-000000000002"), "has.sessions");
        public static readonly GraphTypeRef HasSettings = new(new("b1000000-0000-0000-0000-000000000003"), "has.settings");
        public static readonly GraphTypeRef HasStatus = new(new("b1000000-0000-0000-0000-000000000109"), "has.status");
        public static readonly GraphTypeRef HasTemplates = new(new("b1000000-0000-0000-0000-000000000004"), "has.templates");
        public static readonly GraphTypeRef HasTools = new(new("b1000000-0000-0000-0000-000000000005"), "has.tools");
        public static readonly GraphTypeRef HasWorkflow = new(new("b1000000-0000-0000-0000-000000000107"), "has.workflow");
        public static readonly GraphTypeRef HasWorkspace = new(new("b1000000-0000-0000-0000-000000000106"), "session.has.workspace");
        public static readonly GraphTypeRef HasWorkspaces = new(new("b1000000-0000-0000-0000-000000000001"), "contains.workspaces");
        public static readonly GraphTypeRef ListContainsItem = new(new("b1000000-0000-0000-0000-000000000042"), "list.contains");
        public static readonly GraphTypeRef PackagesHeaderHasPackage = new(new("b1000000-0000-0000-0000-000000000015"), "package.header.has.package");
        public static readonly GraphTypeRef ProducesEvent = new(new("b1000000-0000-0000-0000-000000000203"), "produces.event");
        public static readonly GraphTypeRef ReactsTo = new(new("b1000000-0000-0000-0000-000000000204"), "reacts.to");
        public static readonly GraphTypeRef ScenarioHasScenarioStep = new(new("b1000000-0000-0000-0000-000000000021"), "scenario.has.scenario-step");
        public static readonly GraphTypeRef ScenarioStepHasAction = new(new("b1000000-0000-0000-0000-000000000022"), "scenario-step.has.action");
        public static readonly GraphTypeRef ScenarioStepHasEvent = new(new("b1000000-0000-0000-0000-000000000024"), "scenario-step.has.event");
        public static readonly GraphTypeRef ScenarioStepHasHandler = new(new("b1000000-0000-0000-0000-000000000023"), "scenario-step.has.handler");
        public static readonly GraphTypeRef SettingsHasActivity = new(new("b1000000-0000-0000-0000-000000000043"), "settings.has.activity");
        public static readonly GraphTypeRef SettingsHasApiKey = new(new("b1000000-0000-0000-0000-000000000214"), "settings.has.api-key");
        public static readonly GraphTypeRef SettingsHasOutbox = new(new("b1000000-0000-0000-0000-000000000044"), "settings.has.outbox");
        public static readonly GraphTypeRef StepsHeaderHasStep = new(new("b1000000-0000-0000-0000-000000000016"), "step.header.has.step");
        public static readonly GraphTypeRef UseCaseHasUserStory = new(new("b1000000-0000-0000-0000-000000000019"), "use-case.has.user-story");
        public static readonly GraphTypeRef UserStoryHasScenario = new(new("b1000000-0000-0000-0000-000000000020"), "user-story.has.scenario");
        public static readonly GraphTypeRef WorkspacesHeaderHasWorkspace = new(new("b1000000-0000-0000-0000-000000000017"), "has.workspace");
    }
}
