namespace LowlandTech.Core.Plugins;

/// <summary>
/// User context probe consulted by <see cref="INavigationService"/> to gate pages by RBAC.
/// </summary>
public interface INavigationUser
{
    bool IsInRole(string role);
}

/// <summary>
/// Builds a <see cref="NavigationTree"/> for the given user from registered page descriptors,
/// filtered by enabled features and RBAC, grouped and sorted, with ShowInNav respected.
/// SPEC2050 US06.
/// </summary>
public interface INavigationService
{
    NavigationTree BuildForUser(INavigationUser user);
}

public sealed class NavigationService(
    IPageTypeRegistry pages,
    IFeatureRegistry features) : INavigationService
{
    public NavigationTree BuildForUser(INavigationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var enabledFeatureIds = new HashSet<string>(
            features.All.Where(f => f.IsEnabled).Select(f => f.Id),
            StringComparer.OrdinalIgnoreCase);

        var visible = pages.All
            .Where(p => p.ShowInNav)
            .Where(p => enabledFeatureIds.Contains(p.FeatureId))
            .Where(p => p.RequiredRoles.Length == 0 || p.RequiredRoles.Any(user.IsInRole))
            .ToArray();

        // Group by NavGroup (null group becomes "")
        var groups = visible
            .GroupBy(p => p.NavGroup ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(groupGrouping =>
            {
                var groupName = groupGrouping.Key;

                // First-registered page in the group provides the group icon (when set).
                var groupIcon = groupGrouping
                    .OrderBy(p => p.NavOrder)
                    .ThenBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(p => p.NavGroupIcon)
                    .FirstOrDefault(icon => !string.IsNullOrWhiteSpace(icon));

                var sections = groupGrouping
                    .GroupBy(p => p.NavSection, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(sectionGrouping => new NavigationSection(
                        Name: sectionGrouping.Key,
                        Items: sectionGrouping
                            .OrderBy(p => p.NavOrder)
                            .ThenBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
                            .Select(p => new NavigationItem(
                                PageId: p.Id,
                                FeatureId: p.FeatureId,
                                Route: p.Route,
                                Title: p.DisplayName,
                                Icon: p.NavIcon,
                                Order: p.NavOrder))
                            .ToArray()))
                    .ToArray();

                return new NavigationGroup(groupName, groupIcon, sections);
            })
            .ToArray();

        return new NavigationTree(groups);
    }
}
