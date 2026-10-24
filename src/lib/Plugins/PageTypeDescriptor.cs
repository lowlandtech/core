namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declarative description of a Blazor page contributed by a feature.
/// SPEC2050 US06.
/// </summary>
public sealed record PageTypeDescriptor
{
    public required string Id { get; init; }
    public required string FeatureId { get; init; }

    /// <summary>The Blazor component type rendered at <see cref="Route"/>.</summary>
    public required Type ComponentType { get; init; }

    /// <summary>The route template, e.g. <c>/admin/realms/{realmId}</c>.</summary>
    public required string Route { get; init; }

    /// <summary>Canonical page name from the SC00 contract.</summary>
    public string? Name { get; init; }

    /// <summary>Title shown in the navigation tree.</summary>
    public string? Title { get; init; }

    /// <summary>Optional longer description shown in docs or admin UI.</summary>
    public string? Description { get; init; }

    /// <summary>Functional area from the SC00 page contract.</summary>
    public string? Area { get; init; }

    /// <summary>Roles required to see this page in nav AND to access its route.</summary>
    public string[] RequiredRoles { get; init; } = [];

    /// <summary>Permission keys required to access the page.</summary>
    public string[] RequiredPermissions { get; init; } = [];

    /// <summary>Authorization policies required to access the page.</summary>
    public string[] RequiredPolicies { get; init; } = [];

    /// <summary>Markdown spec path that defines the page contract.</summary>
    public string? SpecPath { get; init; }

    /// <summary>Named slots declared by the page's SC00 contract.</summary>
    public PageSlotDescriptor[] Slots { get; init; } = [];

    /// <summary>Top-level navigation group, e.g. <c>"Admin"</c>.</summary>
    public string? NavGroup { get; init; }

    /// <summary>Optional sub-section within the nav group.</summary>
    public string? NavSection { get; init; }

    /// <summary>Sort order within the section. Lower values appear first.</summary>
    public int NavOrder { get; init; }

    /// <summary>Icon shown next to the page in the nav tree.</summary>
    public string? NavIcon { get; init; }

    /// <summary>Icon used for the entire <see cref="NavGroup"/> when this is the first registered page in it.</summary>
    public string? NavGroupIcon { get; init; }

    /// <summary>If false, the page is reachable by route but does NOT appear in the nav tree.</summary>
    public bool ShowInNav { get; init; } = true;

    /// <summary>Preferred display name used by navigation and admin surfaces.</summary>
    public string DisplayName => Title ?? Name ?? Id;
}
