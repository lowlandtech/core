namespace LowlandTech.Core;

public interface IComponentFlowExecutor
{
    Task<FlowExecutionResult> ExecuteAllAsync(string rootNamespace, CancellationToken ct = default);
    Task<FlowExecutionResult> ExecuteOneAsync(string containerName, string rootNamespace, CancellationToken ct = default);
}
