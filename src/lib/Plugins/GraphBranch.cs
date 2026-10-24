namespace LowlandTech.Core.Plugins;

/// <summary>
/// In-process implementation of <see cref="IGraphBranch"/> that:
/// <list type="bullet">
///   <item>Creates a SQLite file at <c>{branchesRoot}/{type}_{sessionId}_{timestamp}.db</c>.</item>
///   <item>Records provisional transitions in memory (no actual EF schema required for testing).</item>
///   <item>Produces <see cref="GraphMergeRequest"/> on commit without touching any target database.</item>
/// </list>
/// SPEC2050 US05.
/// </summary>
public sealed class GraphBranch : IGraphBranch
{
    private readonly Dictionary<string, IGraphEdgeToolAdapter> _tools;
    private readonly List<ProposedTransition> _transitions = [];

    public Guid BranchId { get; }
    public string Goal { get; }
    public BranchType BranchType { get; }
    public BranchStatus Status { get; private set; } = BranchStatus.Active;
    public string SqlitePath { get; }
    public IReadOnlyList<ProposedTransition> ProposedTransitions => _transitions.AsReadOnly();

    public GraphBranch(
        string sessionId,
        string goal,
        BranchType branchType,
        string branchesRoot,
        IEnumerable<IGraphEdgeToolAdapter>? tools = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentException.ThrowIfNullOrEmpty(goal);
        ArgumentException.ThrowIfNullOrEmpty(branchesRoot);

        BranchId = Guid.NewGuid();
        Goal = goal;
        BranchType = branchType;
        _tools = (tools ?? []).ToDictionary(t => t.Verb, StringComparer.OrdinalIgnoreCase);

        var prefix = branchType.ToString().ToLowerInvariant();
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var fileName = $"{prefix}_{sessionId}_{stamp}.db";
        SqlitePath = Path.Combine(branchesRoot, fileName);

        Directory.CreateDirectory(branchesRoot);
        // Touch the file — downstream code or real SQLite bootstrap can seed it.
        File.WriteAllBytes(SqlitePath, []);
    }

    /// <inheritdoc/>
    public async Task<GraphTransitionResult> TryTransitionAsync(
        object source,
        string verb,
        TransitionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(verb);
        ArgumentNullException.ThrowIfNull(context);

        if (Status != BranchStatus.Active)
            return GraphTransitionResult.Failed(
                TransitionPhase.Precondition,
                $"Branch is not active (current status: {Status}).");

        if (!_tools.TryGetValue(verb, out var adapter))
            return GraphTransitionResult.Failed(
                TransitionPhase.Precondition,
                $"No tool registered for verb '{verb}'.");

        var result = await adapter.InvokeAsync(source, context, cancellationToken).ConfigureAwait(false);

        if (result.Success)
        {
            _transitions.Add(new ProposedTransition
            {
                Ordinal = _transitions.Count + 1,
                Verb = verb,
                SourceNodeType = source.GetType().Name,
                TargetNodeType = result.Target?.GetType().Name ?? string.Empty,
                ProvisionalId = Guid.NewGuid(),
                Parameters = context.Parameters,
                SourceSnapshot = source,
                TargetSnapshot = result.Target!,
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public Task<GraphMergeRequest> CommitAsync(CancellationToken cancellationToken = default)
    {
        if (Status != BranchStatus.Active)
            throw new InvalidOperationException(
                $"Cannot commit a branch with status {Status}.");

        Status = BranchStatus.Committed;

        var targetBranch = BranchType switch
        {
            BranchType.Hotfix => BranchType.Feature,   // hotfix → main (Feature = develop; hotfix targets main)
            BranchType.Release => BranchType.Release,
            _ => BranchType.Feature,
        };

        var mr = new GraphMergeRequest
        {
            Id = Guid.NewGuid(),
            BranchId = BranchId,
            TargetBranch = BranchType,
            Status = MergeRequestStatus.Open,
            ProposedTransitions = _transitions.ToList().AsReadOnly(),
            SqlitePath = SqlitePath,
        };

        return Task.FromResult(mr);
    }

    /// <inheritdoc/>
    public Task<FeatureProposalDocument> ProposeAsync(
        IReadOnlyList<FeatureProposal> proposals,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposals);

        Status = BranchStatus.Proposed;

        var doc = new FeatureProposalDocument
        {
            Id = Guid.NewGuid(),
            BranchId = BranchId,
            ValidatedPath = _transitions.Select(t => t.Verb).ToList(),
            Proposals = proposals,
        };

        return Task.FromResult(doc);
    }

    /// <inheritdoc/>
    public Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        Status = BranchStatus.Abandoned;

        if (File.Exists(SqlitePath))
            File.Delete(SqlitePath);

        return Task.CompletedTask;
    }
}
