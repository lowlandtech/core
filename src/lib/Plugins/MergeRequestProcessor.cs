namespace LowlandTech.Core.Plugins;

/// <summary>
/// Processes approved and rejected <see cref="GraphMergeRequest"/> objects:
/// <list type="bullet">
///   <item><b>Approve</b>: replays every <see cref="ProposedTransition"/> against the target
///     database (via injected tool adapters), resolves provisional IDs, archives the SQLite
///     branch file, and emits <see cref="GraphMergeRequestMerged"/>.</item>
///   <item><b>Reject</b>: archives the branch file without executing any tools.</item>
///   <item><b>Hotfix back-merge</b>: after approving a hotfix MR, automatically creates
///     a back-merge <see cref="GraphMergeRequest"/> targeting develop.</item>
/// </list>
/// SPEC2050 US05.
/// </summary>
public sealed class MergeRequestProcessor
{
    private readonly IReadOnlyDictionary<string, IGraphEdgeToolAdapter> _tools;

    public MergeRequestProcessor(IEnumerable<IGraphEdgeToolAdapter> tools)
    {
        ArgumentNullException.ThrowIfNull(tools);
        _tools = tools.ToDictionary(t => t.Verb, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Replay the branch transitions against the target, archive the SQLite file, and emit
    /// the <see cref="GraphMergeRequestMerged"/> event.
    /// </summary>
    /// <param name="mergeRequest">Open merge request to approve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// The domain event plus, for hotfix branches, an automatically-created back-merge MR.
    /// </returns>
    public async Task<MergeRequestApprovalResult> ApproveAsync(
        GraphMergeRequest mergeRequest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        if (mergeRequest.Status != MergeRequestStatus.Open)
            throw new InvalidOperationException(
                $"Cannot approve a merge request with status {mergeRequest.Status}.");

        var resolvedIds = new List<(Guid ProvisionalId, Guid RealId)>();

        foreach (var transition in mergeRequest.ProposedTransitions)
        {
            if (!_tools.TryGetValue(transition.Verb, out var adapter))
                throw new InvalidOperationException(
                    $"No tool adapter registered for verb '{transition.Verb}'.");

            var context = new TransitionContext(parameters: transition.Parameters);
            var result = await adapter.InvokeAsync(
                transition.SourceSnapshot, context, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
                throw new InvalidOperationException(
                    $"Tool '{transition.Verb}' failed during replay: {result.FailureReason}");

            var realId = Guid.NewGuid();
            resolvedIds.Add((transition.ProvisionalId, realId));
        }

        mergeRequest.Status = MergeRequestStatus.Approved;
        ArchiveSqlite(mergeRequest.SqlitePath);

        var mergedEvent = new GraphMergeRequestMerged(
            mergeRequest.Id,
            mergeRequest.TargetBranch,
            resolvedIds.AsReadOnly());

        // Hotfix back-merge: auto-create a develop-targeting MR with the same transitions.
        GraphMergeRequest? backMergeMr = null;
        if (mergeRequest.TargetBranch == BranchType.Hotfix)
        {
            backMergeMr = new GraphMergeRequest
            {
                Id = Guid.NewGuid(),
                BranchId = mergeRequest.BranchId,
                TargetBranch = BranchType.Feature, // Feature = develop
                Status = MergeRequestStatus.Open,
                ProposedTransitions = mergeRequest.ProposedTransitions,
            };
        }

        return new MergeRequestApprovalResult(mergedEvent, backMergeMr);
    }

    /// <summary>
    /// Archive the SQLite file without executing any tools.
    /// </summary>
    public Task RejectAsync(GraphMergeRequest mergeRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        if (mergeRequest.Status != MergeRequestStatus.Open)
            throw new InvalidOperationException(
                $"Cannot reject a merge request with status {mergeRequest.Status}.");

        mergeRequest.Status = MergeRequestStatus.Rejected;
        ArchiveSqlite(mergeRequest.SqlitePath);
        return Task.CompletedTask;
    }

    private static void ArchiveSqlite(string? path)
    {
        if (path is null || !File.Exists(path)) return;
        var archive = path + ".archived";
        File.Move(path, archive, overwrite: true);
    }
}

/// <summary>
/// Result returned by <see cref="MergeRequestProcessor.ApproveAsync"/>.
/// </summary>
public sealed record MergeRequestApprovalResult(
    GraphMergeRequestMerged MergedEvent,
    GraphMergeRequest? BackMergeMr);
