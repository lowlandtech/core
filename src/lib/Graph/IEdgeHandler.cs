
namespace LowlandTech.Core.Graph;

/// <summary>
/// Executes the state transition for an edge intent.
/// EdgeType.ClrType resolves to an implementation via DI.
///
/// Execution pipeline:
/// 1. Edge created (intent declared)
/// 2. IRuleEvaluator validates all rules on source/target/edge properties
/// 3. If valid → ExecuteAsync is called
/// 4. If invalid → edge is rejected with RuleViolations
///
/// Handlers never need to validate — rules have already passed.
/// </summary>
public interface IEdgeHandler
{
    /// <summary>
    /// Execute the intent. Called only after all rules have passed.
    /// </summary>
    Task ExecuteAsync(Edge edge, CancellationToken ct = default);

    /// <summary>
    /// Reverse the intent — called when an edge is deleted.
    /// Not all intents are reversible; implementations may no-op.
    /// </summary>
    Task ReverseAsync(Edge edge, CancellationToken ct = default);
}
