namespace LowlandTech.Core.Contracts.Plugins;

/// <summary>
/// The portable target of a plugin. The two members are the target meanings the
/// published hosts already use, so a host that predates the portable contract
/// still reads a portable manifest.
/// </summary>
public enum PortablePluginTarget
{
    /// <summary>The backend host.</summary>
    Backend,

    /// <summary>The frontend host.</summary>
    Blazor
}

/// <summary>
/// The versioned extension of a portable target. A new meaning of a target is
/// an extension of a versioned declaration and not a new member of the target
/// enum, because a host that predates the extension has to keep reading the
/// older meaning of the same target.
/// </summary>
public enum PortablePluginTargetExtension
{
    /// <summary>The target carries no extension and keeps its plain meaning.</summary>
    None,

    /// <summary>The target is a backend API contribution.</summary>
    BackendApi,

    /// <summary>The target is a frontend contribution.</summary>
    Frontend
}

/// <summary>
/// The target of a plugin as a versioned declaration. The version tells a
/// consumer which meaning of the target it reads, and the extension carries a
/// new meaning of that target without changing the target itself.
/// </summary>
public sealed record PortablePluginTargetDeclaration(
    PortablePluginTarget Target,
    int Version,
    PortablePluginTargetExtension Extension)
{
    /// <summary>The version of the plain target meaning.</summary>
    public const int LegacyVersion = 1;

    /// <summary>The plain backend target.</summary>
    public static readonly PortablePluginTargetDeclaration PlainBackend =
        new(PortablePluginTarget.Backend, LegacyVersion, PortablePluginTargetExtension.None);

    /// <summary>The plain frontend target.</summary>
    public static readonly PortablePluginTargetDeclaration PlainBlazor =
        new(PortablePluginTarget.Blazor, LegacyVersion, PortablePluginTargetExtension.None);
}

/// <summary>
/// The installation unit of a plugin. The whole package is one inseparable
/// installation unit, so every capability the unit names is required.
/// </summary>
public sealed record PortablePluginInstallationUnit(
    IReadOnlyList<string> RequiredCapabilities,
    IReadOnlyDictionary<string, string> RequiredContracts);

/// <summary>A logical plugin dependency. Package resolution stays with NuGet.</summary>
public sealed record PortablePluginDependencyDeclaration(string Key);

/// <summary>A declared route of a plugin.</summary>
public sealed record PortablePluginRouteDeclaration(
    string Path,
    string? Component,
    string? RedirectTo,
    IReadOnlyList<PortablePluginRouteDeclaration> Children,
    IReadOnlyList<string> Policies);

/// <summary>A declared slot of a plugin.</summary>
public sealed record PortablePluginSlotDeclaration(string Key, string ComponentHost);

/// <summary>The kind of a declared placement parameter.</summary>
public enum PortablePluginParameterKind
{
    /// <summary>The parameter carries text.</summary>
    Text,

    /// <summary>The parameter carries a number.</summary>
    Number,

    /// <summary>The parameter carries a flag.</summary>
    Flag,

    /// <summary>The parameter carries no value.</summary>
    Null
}

/// <summary>
/// A declared placement parameter. The value is data and never a type, so a
/// manifest can be read without resolving anything it names.
/// </summary>
public sealed record PortablePluginParameterValue(PortablePluginParameterKind Kind, string? Text);

/// <summary>A declared component placement of a plugin.</summary>
public sealed record PortablePluginComponentPlacementDeclaration(
    string Key,
    string SlotKey,
    string Component,
    int Order,
    IReadOnlyDictionary<string, PortablePluginParameterValue> Parameters,
    IReadOnlyList<string> Policies);

/// <summary>The client assets a plugin declares as its own metadata.</summary>
public sealed record PortablePluginClientAssetDeclaration(
    IReadOnlyList<string> Stylesheets,
    IReadOnlyList<string> Scripts);

/// <summary>
/// The contributions a plugin declares. Every value is a key and not a type:
/// an owning registry resolves the key and the contract never loads it.
/// </summary>
public sealed record PortablePluginContributionDeclaration(
    IReadOnlyList<string> Clients,
    IReadOnlyList<string> HarnessExtensions,
    IReadOnlyList<string> CliHosts,
    IReadOnlyList<string> CliCommands,
    IReadOnlyList<string> Components,
    IReadOnlyList<string> Themes,
    IReadOnlyList<string> Pages,
    IReadOnlyList<string> Services,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Transitions,
    IReadOnlyList<string> NodeTypes,
    IReadOnlyList<string> DbContexts,
    IReadOnlyList<string> DataServices);

/// <summary>
/// A definition file a plugin declares with the content hash of its bytes. The
/// path and the hash are data, so a consumer can verify a definition file
/// without loading it.
/// </summary>
public sealed record PortablePluginDefinitionFile(string Path, PortablePluginContentHash ContentHash);

/// <summary>
/// The portable declaration of a plugin manifest. It is data only: every value
/// is a string, a number or a flag, and no member carries an executable type
/// that untrusted data could name.
/// </summary>
public sealed record PortablePluginManifestDeclaration(
    int SchemaVersion,
    string Key,
    string Name,
    string Description,
    PortablePluginTargetDeclaration Target,
    PortablePluginInstallationUnit? InstallationUnit,
    IReadOnlyList<PortablePluginDependencyDeclaration> Dependencies,
    IReadOnlyList<PortablePluginRouteDeclaration> Routes,
    IReadOnlyList<PortablePluginSlotDeclaration> Slots,
    IReadOnlyList<PortablePluginComponentPlacementDeclaration> Placements,
    PortablePluginContributionDeclaration Contributions,
    PortablePluginClientAssetDeclaration Assets,
    IReadOnlyList<PortablePluginDefinitionFile> DefinitionFiles);
