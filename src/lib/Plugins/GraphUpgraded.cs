namespace LowlandTech.Core.Plugins;

/// <summary>
/// Domain event emitted when main self-upgrades after a release branch merges.
/// SPEC2050 US05 SC11.
/// </summary>
public sealed record GraphUpgraded(
    IReadOnlyList<string> AppliedFeatureIds,
    IReadOnlyList<string> NewEdgeTypeVerbs,
    IReadOnlyList<string> NewNodeTypeKeys,
    DateTimeOffset UpgradedAt);
