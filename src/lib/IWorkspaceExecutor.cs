namespace LowlandTech.Core;

public interface IWorkspaceExecutor
{
    Task<WorkspaceExecutionResult> ExecuteAsync(Guid workspaceId);
}

public record WorkspaceExecutionResult(List<WorkflowResult> Workflows);

public record WorkflowResult(
    Guid WorkflowId,
    string WorkflowName,
    Guid FlowId,
    string FlowName,
    bool ConditionMet,
    string Conditions,
    FlowExecutionResult? FlowResult);
