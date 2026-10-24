namespace LowlandTech.Core.Plugins;

/// <summary>
/// Domain event emitted after a <see cref="GraphMergeRequest"/> is approved and all
/// branch transitions have been replayed against the target database.
/// SPEC2050 US05 SC04.
/// </summary>
public sealed record GraphMergeRequestMerged(
    Guid MergeRequestId,
    BranchType TargetBranch,
    IReadOnlyList<(Guid ProvisionalId, Guid RealId)> ResolvedIds);
