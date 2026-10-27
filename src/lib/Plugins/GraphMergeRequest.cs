namespace LowlandTech.Core.Plugins;

/// <summary>
/// Represents a pending set of provisional graph transitions awaiting review.
/// Produced by <see cref="IGraphBranch.CommitAsync"/>.
/// SPEC2050 US05.
/// </summary>
public sealed class GraphMergeRequest
{
    public required Guid Id { get; init; }
    public required Guid BranchId { get; init; }

    /// <summary>Which target database this MR was opened against.</summary>
    public required BranchType TargetBranch { get; init; }

    /// <summary>
    /// Current approval state. Mutable so the processor can advance the lifecycle;
    /// a client never assigns a state itself. The default is the open state.
    /// </summary>
    public MergeRequestStatus Status { get; set; } = MergeRequestStatus.Open;

    /// <summary>Ordered list of transitions to replay when approved.</summary>
    public required IReadOnlyList<ProposedTransition> ProposedTransitions { get; init; }

    /// <summary>Path to the originating SQLite branch file. Retained until the MR is resolved.</summary>
    public string? SqlitePath { get; init; }
}
