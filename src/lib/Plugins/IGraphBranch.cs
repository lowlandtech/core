namespace LowlandTech.Core.Plugins;

/// <summary>
/// A disconnected SQLite branch used by an agent to perform provisional graph
/// transitions before committing or abandoning changes to a target database.
/// SPEC2050 US05.
/// </summary>
public interface IGraphBranch
{
    Guid BranchId { get; }

    /// <summary>Human-readable goal for which the branch was opened.</summary>
    string Goal { get; }

    BranchType BranchType { get; }
    BranchStatus Status { get; }

    /// <summary>Absolute path to the disconnected SQLite database file.</summary>
    string SqlitePath { get; }

    /// <summary>Ordered list of provisional transitions recorded in this branch.</summary>
    IReadOnlyList<ProposedTransition> ProposedTransitions { get; }

    /// <summary>
    /// Run <paramref name="verb"/> tool against <paramref name="source"/>; if successful,
    /// record a <see cref="ProposedTransition"/> in the branch SQLite only.
    /// </summary>
    Task<GraphTransitionResult> TryTransitionAsync(
        object source,
        string verb,
        TransitionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Freeze the branch and produce a <see cref="GraphMergeRequest"/> for review.
    /// Does NOT write to develop or main.
    /// </summary>
    Task<GraphMergeRequest> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Emit a <see cref="FeatureProposalDocument"/> for any missing EdgeTypes discovered
    /// during the branch session. Keeps the branch alive (Status = Proposed) for retry.
    /// </summary>
    Task<FeatureProposalDocument> ProposeAsync(
        IReadOnlyList<FeatureProposal> proposals,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Discard the branch immediately — SQLite file is deleted, no merge request created.
    /// </summary>
    Task AbandonAsync(CancellationToken cancellationToken = default);
}
