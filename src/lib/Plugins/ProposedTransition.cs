namespace LowlandTech.Core.Plugins;

/// <summary>
/// A single provisional node + edge recorded by <see cref="IGraphBranch.TryTransitionAsync"/>.
/// SPEC2050 US05.
/// </summary>
public sealed record ProposedTransition
{
    /// <summary>Unique ordinal within the branch (1-based).</summary>
    public required int Ordinal { get; init; }

    /// <summary>The verb/edge key e.g. "create.order".</summary>
    public required string Verb { get; init; }

    /// <summary>CLR type name of the source node fed to TryTransitionAsync.</summary>
    public required string SourceNodeType { get; init; }

    /// <summary>CLR type name of the target node produced by the reduce phase.</summary>
    public required string TargetNodeType { get; init; }

    /// <summary>
    /// Provisional stub ID assigned before the transition is replayed on the real target.
    /// Replaced by a real graph ID when the merge request is approved.
    /// </summary>
    public required Guid ProvisionalId { get; init; }

    /// <summary>Parameters passed to TryTransitionAsync.</summary>
    public required IReadOnlyDictionary<string, object?> Parameters { get; init; }

    /// <summary>Serialised snapshot of the <em>source</em> node as passed to TryTransitionAsync.</summary>
    public required object SourceSnapshot { get; init; }

    /// <summary>Serialised snapshot of the target node produced during the provisional run.</summary>
    public required object TargetSnapshot { get; init; }
}
