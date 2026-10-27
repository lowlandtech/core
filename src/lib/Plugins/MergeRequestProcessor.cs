namespace LowlandTech.Core.Plugins;

/// <summary>
/// Processes the live states of a <see cref="GraphMergeRequest"/>:
/// <list type="bullet">
///   <item><b>Review</b>: an open request enters the review.</item>
///   <item><b>Approve</b>: replays every <see cref="ProposedTransition"/> against the target
///     database (via injected tool adapters), resolves provisional IDs, archives the SQLite
///     branch file, and emits <see cref="GraphMergeRequestMerged"/>.</item>
///   <item><b>Reject</b>: archives the branch file without executing any tools.</item>
///   <item><b>Abandon</b>: archives the branch file without executing any tools.</item>
///   <item><b>Merge</b>: records the terminal merged state of an approved request.</item>
///   <item><b>Hotfix back-merge</b>: after approving a hotfix MR, automatically creates
///     a back-merge <see cref="GraphMergeRequest"/> targeting develop.</item>
/// </list>
/// The processor owns every move between the states, so a client never assigns a
/// state itself and an invalid move is refused here instead of being hidden.
/// SPEC2050 US05; CORE0005-US01-SC01.
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
    /// Whether the request may still be approved.
    /// </summary>
    /// <param name="mergeRequest">The request to read.</param>
    public bool CanApprove(GraphMergeRequest mergeRequest)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        return mergeRequest.Status.CanApprove();
    }

    /// <summary>
    /// Whether the request may still be rejected.
    /// </summary>
    /// <param name="mergeRequest">The request to read.</param>
    public bool CanReject(GraphMergeRequest mergeRequest)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        return mergeRequest.Status.CanReject();
    }

    /// <summary>
    /// Whether the request may still enter the review.
    /// </summary>
    /// <param name="mergeRequest">The request to read.</param>
    public bool CanStartReview(GraphMergeRequest mergeRequest)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        return mergeRequest.Status.CanStartReview();
    }

    /// <summary>
    /// Whether the request may still be abandoned.
    /// </summary>
    /// <param name="mergeRequest">The request to read.</param>
    public bool CanAbandon(GraphMergeRequest mergeRequest)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        return mergeRequest.Status.CanAbandon();
    }

    /// <summary>
    /// Whether the request may be merged.
    /// </summary>
    /// <param name="mergeRequest">The request to read.</param>
    public bool CanMerge(GraphMergeRequest mergeRequest)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        return mergeRequest.Status.CanMerge();
    }

    /// <summary>
    /// Move an open request into the review.
    /// </summary>
    /// <param name="mergeRequest">The request to review.</param>
    /// <param name="cancellationToken"></param>
    public Task StartReviewAsync(
        GraphMergeRequest mergeRequest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        if (!mergeRequest.Status.CanStartReview())
            throw new InvalidOperationException(
                $"Cannot start the review of a merge request with status {mergeRequest.Status}.");

        mergeRequest.Status = MergeRequestStatus.Reviewing;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Replay the branch transitions against the target, archive the SQLite file, and emit
    /// the <see cref="GraphMergeRequestMerged"/> event.
    /// </summary>
    /// <param name="mergeRequest">Open or reviewing merge request to approve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// The domain event plus, for hotfix branches, an automatically-created back-merge MR.
    /// </returns>
    public async Task<MergeRequestApprovalResult> ApproveAsync(
        GraphMergeRequest mergeRequest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        if (!mergeRequest.Status.CanApprove())
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
    /// <param name="mergeRequest">Open or reviewing merge request to reject.</param>
    /// <param name="cancellationToken"></param>
    public Task RejectAsync(GraphMergeRequest mergeRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        if (!mergeRequest.Status.CanReject())
            throw new InvalidOperationException(
                $"Cannot reject a merge request with status {mergeRequest.Status}.");

        mergeRequest.Status = MergeRequestStatus.Rejected;
        ArchiveSqlite(mergeRequest.SqlitePath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Abandon a live request without executing any tools.
    /// </summary>
    /// <param name="mergeRequest">Open or reviewing merge request to abandon.</param>
    /// <param name="cancellationToken"></param>
    public Task AbandonAsync(GraphMergeRequest mergeRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        if (!mergeRequest.Status.CanAbandon())
            throw new InvalidOperationException(
                $"Cannot abandon a merge request with status {mergeRequest.Status}.");

        mergeRequest.Status = MergeRequestStatus.Abandoned;
        ArchiveSqlite(mergeRequest.SqlitePath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Record the terminal merged state of an approved request.
    /// </summary>
    /// <param name="mergeRequest">The approved merge request to merge.</param>
    /// <param name="cancellationToken"></param>
    public Task MergeAsync(GraphMergeRequest mergeRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mergeRequest);
        if (!mergeRequest.Status.CanMerge())
            throw new InvalidOperationException(
                $"Cannot merge a merge request with status {mergeRequest.Status}.");

        mergeRequest.Status = MergeRequestStatus.Merged;
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
