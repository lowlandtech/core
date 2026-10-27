namespace LowlandTech.Core.Plugins;

/// <summary>
/// Lifecycle state of a <see cref="FeatureProposalDocument"/>.
/// CORE0005-US01-SC01.
/// </summary>
/// <remarks>
/// The numeric value of a member is its stable wire value. A document written
/// before the lifecycle existed carries no state and reads as
/// <see cref="Draft"/>, which is the compatible default of a document that was
/// never submitted.
/// </remarks>
public enum FeatureProposalStatus
{
    /// <summary>The proposal was written and may still be submitted or discarded.</summary>
    Draft = 0,

    /// <summary>The proposal was submitted and waits for a review.</summary>
    Reviewed = 1,

    /// <summary>The proposal was reviewed and accepted.</summary>
    Accepted = 2,

    /// <summary>The proposal was reviewed and rejected, or discarded while it was a draft.</summary>
    Rejected = 3,
}

/// <summary>
/// Emitted by <see cref="IGraphBranch.ProposeAsync"/> when an agent encounters a missing
/// EdgeType and stores its progress for a future retry once the feature is implemented.
/// SPEC2050 US05; CORE0005-US01-SC01.
/// </summary>
/// <remarks>
/// A document written before the client contract existed carries no title, no
/// status and no specification. Such a document reads with a null title, a null
/// specification and the <see cref="FeatureProposalStatus.Draft"/> status, which
/// keeps it readable and keeps it submittable. The title, the status and the
/// specification are the three members a client surface renders.
/// </remarks>
public sealed record FeatureProposalDocument
{
    public required Guid Id { get; init; }
    public required Guid BranchId { get; init; }

    /// <summary>Verbs that were successfully validated in the branch before the gap was discovered.</summary>
    public required IReadOnlyList<string> ValidatedPath { get; init; }

    /// <summary>One entry per missing EdgeType.</summary>
    public required IReadOnlyList<FeatureProposal> Proposals { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The stable title of the proposal. Null in a document that predates the title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// The lifecycle state of the proposal. A document that predates the lifecycle
    /// reads as <see cref="FeatureProposalStatus.Draft"/>.
    /// </summary>
    public FeatureProposalStatus Status { get; set; } = FeatureProposalStatus.Draft;

    /// <summary>
    /// The Gherkin specification that describes the gap. Null in a document that
    /// predates the specification.
    /// </summary>
    public string? Specification { get; init; }

    /// <summary>Whether the proposal may still be submitted.</summary>
    public bool CanSubmit => Status == FeatureProposalStatus.Draft;

    /// <summary>Whether the proposal may still be discarded.</summary>
    public bool CanDiscard => Status == FeatureProposalStatus.Draft;

    /// <summary>
    /// Submit the draft. Only a draft may be submitted.
    /// </summary>
    public void Submit()
    {
        if (Status != FeatureProposalStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot submit a feature proposal with status {Status}.");
        Status = FeatureProposalStatus.Reviewed;
    }

    /// <summary>
    /// Discard the draft. Only a draft may be discarded.
    /// </summary>
    public void Discard()
    {
        if (Status != FeatureProposalStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot discard a feature proposal with status {Status}.");
        Status = FeatureProposalStatus.Rejected;
    }

    /// <summary>
    /// Accept the reviewed proposal. Only a reviewed proposal may be accepted.
    /// </summary>
    public void Accept()
    {
        if (Status != FeatureProposalStatus.Reviewed)
            throw new InvalidOperationException(
                $"Cannot accept a feature proposal with status {Status}.");
        Status = FeatureProposalStatus.Accepted;
    }

    /// <summary>
    /// Reject the reviewed proposal. Only a reviewed proposal may be rejected.
    /// </summary>
    public void Reject()
    {
        if (Status != FeatureProposalStatus.Reviewed)
            throw new InvalidOperationException(
                $"Cannot reject a feature proposal with status {Status}.");
        Status = FeatureProposalStatus.Rejected;
    }
}
