
namespace LowlandTech.Core.Abstractions;

public interface IWorkflowRunService
{
    Task<WorkflowRun?> GetLatestCompletedAsync(Guid workflowId, CancellationToken ct = default);
    Task<WorkflowRun> CreateAsync(WorkflowRun run, CancellationToken ct = default);
    Task<WorkflowRun> UpdateAsync(WorkflowRun run, CancellationToken ct = default);
    Task AddStepAsync(WorkflowRunStep step, CancellationToken ct = default);
}


