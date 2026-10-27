using System.Diagnostics.CodeAnalysis;

namespace LowlandTech.Core.Plugins;

/// <summary>
/// Identity, contract binding, and activation state for a feature contributed by a plugin.
/// SPEC2050 US03.
/// </summary>
public sealed record FeatureDescriptor
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string? SpecPath { get; init; }

    /// <summary>Plugin id that owns this feature.</summary>
    public required string PluginId { get; init; }

    /// <summary>The service contract this feature binds.</summary>
    public required Type ContractType { get; init; }

    /// <summary>The implementation type bound to <see cref="ContractType"/>.</summary>
    public required Type ImplementationType { get; init; }

    /// <summary>Permission keys callers must hold to invoke this feature's contract.</summary>
    public string[] RequiredPermissions { get; init; } = [];

    /// <summary>Role keys callers must hold to invoke this feature's contract.</summary>
    public string[] RequiredRoles { get; init; } = [];

    /// <summary>Authorization policies that gate this feature.</summary>
    public string[] RequiredPolicies { get; init; } = [];

    /// <summary>Authentication schemes that must be active for this feature.</summary>
    public string[] RequiredAuthenticationSchemes { get; init; } = [];

    /// <summary>
    /// Desired activation state when the feature is first registered.
    /// The current runtime state is reflected by <see cref="IsEnabled"/>.
    /// </summary>
    public bool EnabledByDefault { get; init; } = true;

    /// <summary>Optional noun-verb edge contract owned by this feature.</summary>
    public EdgeTypeDescriptor? EdgeType { get; init; }

    /// <summary>Optional shared settings type for this feature.</summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? SettingsType { get; init; }

    /// <summary>Pages contributed by this feature.</summary>
    public PageTypeDescriptor[] Pages { get; init; } = [];

    /// <summary>Blocks contributed by this feature.</summary>
    public BlockTypeDescriptor[] Blocks { get; init; } = [];

    /// <summary>Background jobs contributed by this feature.</summary>
    public Type[] BackgroundJobTypes { get; init; } = [];

    /// <summary>Event subscribers contributed by this feature.</summary>
    public Type[] EventSubscriberTypes { get; init; } = [];

    /// <summary>MCP tools contributed by this feature.</summary>
    public Type[] McpToolTypes { get; init; } = [];

    /// <summary>True if the feature is currently enabled and resolvable.</summary>
    public bool IsEnabled { get; init; } = true;
}
