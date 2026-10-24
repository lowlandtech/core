namespace LowlandTech.Core.Plugins;

/// <summary>
/// Lifecycle state of a graph branch.
/// SPEC2050 US05.
/// </summary>
public enum BranchStatus
{
    /// <summary>Branch is open; agent is performing provisional transitions.</summary>
    Active,

    /// <summary>CommitAsync has been called; a GraphMergeRequest is awaiting review.</summary>
    Committed,

    /// <summary>ProposeAsync was called because a required EdgeType is missing.</summary>
    Proposed,

    /// <summary>The merge request was approved and tools have been replayed against the target.</summary>
    Merged,

    /// <summary>The branch or merge request was rejected/abandoned.</summary>
    Abandoned,
}
