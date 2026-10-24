namespace LowlandTech.Core;

public interface IFlowExecutor
{
    Task<FlowExecutionResult> ExecuteAsync(
        Guid flowId, Guid workspaceId, string? workspacePath = null,
        Dictionary<(Guid StepId, Guid? TableId), string>? previousHashes = null);
}

public record FlowExecutionResult(
    List<FileOutput> Files,
    List<CommandOutput> Commands,
    List<SkippedOutput> Skipped,
    List<UnchangedOutput> Unchanged);

public record FileOutput(string Path, string Content, string StepName, Guid StepId, Guid? TableId, string ContentHash);
public record CommandOutput(string Command, string StepName, Guid StepId, Guid? TableId, string? WorkingDir = null);
public record SkippedOutput(string StepName, Guid StepId, Guid? TableId, string Reason);
public record UnchangedOutput(string StepName, Guid StepId, Guid? TableId, string Path, string ContentHash);
