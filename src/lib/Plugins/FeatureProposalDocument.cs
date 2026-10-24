namespace LowlandTech.Core.Plugins;

/// <summary>
/// Emitted by <see cref="IGraphBranch.ProposeAsync"/> when an agent encounters a missing
/// EdgeType and stores its progress for a future retry once the feature is implemented.
/// SPEC2050 US05.
/// </summary>
public sealed record FeatureProposalDocument
{
    public required Guid Id { get; init; }
    public required Guid BranchId { get; init; }

    /// <summary>Verbs that were successfully validated in the branch before the gap was discovered.</summary>
    public required IReadOnlyList<string> ValidatedPath { get; init; }

    /// <summary>One entry per missing EdgeType.</summary>
    public required IReadOnlyList<FeatureProposal> Proposals { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
