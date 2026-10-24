
namespace LowlandTech.Core.Graph;

public record EdgeExecutionResult(bool Success, Edge? Edge, RuleResult? Validation);

/// <summary>
/// Orchestrates the edge execution pipeline:
///   1. Resolve EdgeType and its rules
///   2. Collect properties from source node, target node, and edge
///   3. Evaluate all rules via IRuleEvaluator
///   4. If valid → resolve IEdgeHandler from EdgeType.ClrType → ExecuteAsync
///   5. If invalid → return violations, edge is not created
/// </summary>
public interface IGraphExecutor
{
    /// <summary>
    /// Attempt to create an edge and execute its intent.
    /// Returns success with the created edge, or failure with rule violations.
    /// </summary>
    Task<EdgeExecutionResult> CreateEdgeAsync(
        Guid edgeTypeId,
        Guid sourceNodeId,
        Guid targetNodeId,
        IDictionary<string, string?>? edgeProperties = null,
        CancellationToken ct = default);

    /// <summary>
    /// Delete an edge and reverse its intent if the handler supports it.
    /// </summary>
    Task DeleteEdgeAsync(Guid edgeId, CancellationToken ct = default);
}
