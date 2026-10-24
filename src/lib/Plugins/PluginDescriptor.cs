
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Identity, lifecycle, and dependency metadata for a plugin.
/// SPEC2050 US01 (registration) + US14 (bootstrap, dependencies, shared contracts).
/// </summary>
public sealed record PluginDescriptor
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public string? Author { get; init; }
    public string? Icon { get; init; }

    /// <summary>
    /// Description shown to administrators.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Markdown documentation path used by the Obsidian-backed knowledge base.
    /// </summary>
    public string? DocumentationPath { get; init; }

    /// <summary>
    /// Markdown spec path that defines this plugin's contract.
    /// </summary>
    public string? SpecPath { get; init; }

    /// <summary>
    /// Install mechanisms this plugin supports. Defaults to
    /// <see cref="PluginInstallMode.Routed"/>. The host resolves an
    /// <see cref="IPluginInstaller"/> by matching its declared mode against
    /// this array; if the intersection with the host-supported set is empty,
    /// install is rejected at plan time (US20 SC04).
    /// SPEC2050 US20.
    /// </summary>
    public PluginInstallMode[] InstallModes { get; init; } = [PluginInstallMode.Routed];

    /// <summary>
    /// Hard runtime dependencies on other plugins.
    /// The host enforces load order and refuses to load when a dependency is missing.
    /// </summary>
    public PluginDependency[] DependsOn { get; init; } = [];

    /// <summary>
    /// Fully qualified type names of public abstractions this plugin exposes
    /// to other plugins (must come from a shared abstractions assembly loaded
    /// in the default <c>AssemblyLoadContext</c>).
    /// </summary>
    public string[] ProvidesContracts { get; init; } = [];

    /// <summary>
    /// Shared contracts package/assembly referenced by both runtime packages.
    /// </summary>
    public string? CorePackageId { get; init; }

    /// <summary>
    /// Host-installed backend runtime package id.
    /// </summary>
    public string? BackendPackageId { get; init; }

    /// <summary>
    /// Host-installed frontend runtime package id.
    /// </summary>
    public string? FrontendPackageId { get; init; }

    /// <summary>
    /// Base namespace used for resources and localizers contributed by the plugin.
    /// </summary>
    public string? ResourceNamespace { get; init; }

    /// <summary>
    /// Searchable tags for marketplace and developer tooling.
    /// </summary>
    public string[] Tags { get; init; } = [];

    /// <summary>
    /// CLR type of the plugin's <c>DbContext</c>, or <c>null</c> for stateless plugins.
    /// </summary>
    public Type? DbContextType { get; init; }

    /// <summary>
    /// PostgreSQL schema name owned by this plugin (typically the plugin id, lowercase).
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Absolute path to the staging directory containing the plugin's primary assembly.
    /// Required for <see cref="LowlandTech.Core.Plugins.HotReload.IAlcLoader"/> isolation:
    /// when set and an <see cref="LowlandTech.Core.Plugins.HotReload.IAlcLoader"/> is
    /// registered, <see cref="PluginHostService"/> loads the assembly from this path into
    /// a collectible <see cref="System.Runtime.Loader.AssemblyLoadContext"/> instead of
    /// the default in-process reference.
    /// SPEC2050 US06 / US08 SC10.
    /// </summary>
    public string? AssemblyPath { get; init; }
}
