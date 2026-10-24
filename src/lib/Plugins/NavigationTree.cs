namespace LowlandTech.Core.Plugins;

/// <summary>
/// Result of <see cref="INavigationService.BuildForUser"/>: pages grouped by NavGroup,
/// then by NavSection, then sorted by NavOrder.
/// SPEC2050 US06.
/// </summary>
public sealed record NavigationTree(IReadOnlyList<NavigationGroup> Groups);

public sealed record NavigationGroup(
    string Name,
    string? Icon,
    IReadOnlyList<NavigationSection> Sections);

public sealed record NavigationSection(
    string? Name,
    IReadOnlyList<NavigationItem> Items);

public sealed record NavigationItem(
    string PageId,
    string FeatureId,
    string Route,
    string Title,
    string? Icon,
    int Order);
