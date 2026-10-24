namespace LowlandTech.Core.Plugins;

/// <summary>
/// A single missing-EdgeType proposal recorded by an agent before calling
/// <see cref="IGraphBranch.ProposeAsync"/>.
/// SPEC2050 US05.
/// </summary>
public sealed record FeatureProposal
{
    /// <summary>The missing edge verb, e.g. <c>"notify.customer"</c>.</summary>
    public required string EdgeKey { get; init; }

    /// <summary>Optional human-readable description of the required capability.</summary>
    public string? Description { get; init; }
}
