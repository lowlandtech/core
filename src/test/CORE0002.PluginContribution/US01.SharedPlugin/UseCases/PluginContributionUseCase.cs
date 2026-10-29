using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Nodes;
using LowlandTech.Core.Contracts.Plugins;

namespace LowlandTech.Core.Tests.CORE0002.PluginContribution.US01.SharedPlugin.UseCases;

/// <summary>
/// The acceptance inventory of CORE0002-US02-SC01. The fixtures are the exact
/// manifest documents the Plugins lane reported: the version 1 manifest of a
/// published package and the three version 2 and version 3 shapes its writer
/// accepts and its inspector refuses. Every observation here is of the real
/// documents and not of a shape invented by this lane.
///
/// The observations are captured here so that every Fact of the scenario
/// asserts one of them against the production contract.
/// </summary>
public sealed class PluginContributionUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    private const string FixtureFolder = "CORE0002.US02.SharedPlugin";

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The portable declarations carry no runtime type names.</summary>
    public bool? PortableDeclarationsCarryNoRuntimeTypeNames { get; private set; }

    /// <summary>The consumers use one versioned contract.</summary>
    public bool? ConsumersUseOneVersionedContract { get; private set; }

    /// <summary>The package-only consumer validates the declarations.</summary>
    public bool? PackageOnlyConsumerValidatesDeclarations { get; private set; }

    /// <summary>The contract carries no host or EF dependency.</summary>
    public bool? ContractHasNoHostOrEfDependencies { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "The use case reads the public properties of its own contract types to prove that none of them carries System.Type. The test assembly is not trimmed and the contract assembly it reads is.")]
    public Task InvokeAsync()
    {
        // A versioned backend api contribution with the definition files it
        // declares and the content hash of their bytes.
        var content = Encoding.UTF8.GetBytes("{\"kind\":\"backend-api\",\"version\":2}");
        var hash = PortablePluginContentHash.Compute("sha256", content);
        var backendApi = $$"""
            {
              "schemaVersion": 2, "key": "sample.backend.api", "name": "Sample Backend API",
              "description": "The versioned backend api contribution of the host",
              "target": { "hostTarget": "backend", "version": 2, "extension": "backendApi" },
              "dependencies": [], "routes": [], "slots": [], "placements": [],
              "contributions": {
                "capabilities": ["Sample.Components.DoesNotExist"],
                "dbContexts": ["Sample.Data.DoesNotExist"]
              },
              "installationUnit": { "requiredCapabilities": ["sample"], "requiredContracts": {} },
              "definitionFiles": [{ "path": "definitions/backend-api.json", "contentHash": { "algorithm": "{{hash.Algorithm}}", "digest": "{{hash.Digest}}" } }]
            }
            """;
        var read = PortablePluginManifestValidator.Read(backendApi);
        var manifest = read.Manifest;
        var frontend = """
            {
              "schemaVersion": 2, "key": "sample.frontend", "name": "Sample Frontend",
              "description": "The versioned frontend contribution of the host",
              "target": { "hostTarget": "blazor", "version": 2, "extension": "frontend" },
              "dependencies": [], "routes": [], "slots": [], "placements": [],
              "contributions": { "capabilities": ["sample.frontend"] },
              "installationUnit": { "requiredCapabilities": ["sample", "blazor"], "requiredContracts": {} }
            }
            """;
        var frontendRead = PortablePluginManifestValidator.Read(frontend);

        // No member of the declaration carries a type, so the declaration cannot
        // hold an executable type that untrusted data named.
        var noRuntimeTypes = true;
        foreach (var type in new[]
                 {
                     typeof(PortablePluginManifestDeclaration),
                     typeof(PortablePluginContributionDeclaration),
                     typeof(PortablePluginTargetDeclaration),
                     typeof(PortablePluginDefinitionFile),
                     typeof(PortablePluginParameterValue)
                 })
        {
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType == typeof(Type)) noRuntimeTypes = false;
            }
        }

        var declarations = manifest is not null
            && read.IsAccepted
            && manifest.Target.Target == PortablePluginTarget.Backend
            && manifest.Target.Version == 2
            && manifest.Target.Extension == PortablePluginTargetExtension.BackendApi
            && PortablePluginTargetDeclaration.PlainBackend.Target == PortablePluginTarget.Backend
            && PortablePluginTargetDeclaration.PlainBackend.Extension == PortablePluginTargetExtension.None
            && manifest.DefinitionFiles.Count == 1
            && manifest.DefinitionFiles[0].Path == "definitions/backend-api.json"
            && manifest.DefinitionFiles[0].ContentHash.Algorithm == hash.Algorithm
            && manifest.DefinitionFiles[0].ContentHash.Digest == hash.Digest
            && manifest.Contributions.Capabilities.Single() == "Sample.Components.DoesNotExist"
            && manifest.Contributions.DbContexts.Single() == "Sample.Data.DoesNotExist"
            && frontendRead.IsAccepted
            && frontendRead.Manifest!.Target.Target == PortablePluginTarget.Blazor
            && frontendRead.Manifest!.Target.Version == 2
            && frontendRead.Manifest!.Target.Extension == PortablePluginTargetExtension.Frontend
            && noRuntimeTypes;
        Record("PortableDeclarationsCarryNoRuntimeTypeNames", declarations,
            "The versioned backend and frontend declarations were read and no declaration member carries a type.");

        // The manifest of a published version 1 package stays readable and the
        // declaration writes a document the contract reads back unchanged.
        var legacyRead = PortablePluginManifestValidator.Read(ReadFixture("manifest-v1.json"));
        var legacyManifest = legacyRead.Manifest;
        var written = legacyManifest is null ? null : PortablePluginManifestWriter.Write(legacyManifest);
        var reread = written is null ? null : PortablePluginManifestValidator.Read(written);
        var rewritten = reread?.Manifest is null ? null : PortablePluginManifestWriter.Write(reread.Manifest);
        var roundTrip = written is not null && rewritten is not null
                        && JsonNode.DeepEquals(JsonNode.Parse(written), JsonNode.Parse(rewritten));

        // The version 2 and version 3 shapes the Plugins writer accepts and its
        // inspector refuses receive the named versioned refusal of the contract.
        var v2Empty = PortablePluginManifestValidator.Read(ReadFixture("manifest-v2-empty.json"));
        var v2Cli = PortablePluginManifestValidator.Read(ReadFixture("manifest-v2-cli.json"));
        var v3Assets = PortablePluginManifestValidator.Read(ReadFixture("manifest-v3-assets.json"));
        var assembly = typeof(PortablePluginManifestValidator).Assembly;
        var version = InformationalVersion(assembly);
        var oneContract = legacyRead.IsAccepted
                          && legacyManifest is not null
                          && legacyManifest.SchemaVersion == 1
                          && legacyManifest.Key == "sample.ui"
                          && legacyManifest.Name == "Sample UI"
                          && legacyManifest.Target.Target == PortablePluginTarget.Blazor
                          && legacyManifest.Target.Version == PortablePluginTargetDeclaration.LegacyVersion
                          && legacyManifest.Contributions.Components.Single() == "Sample.Components.Details"
                          && legacyManifest.Contributions.DataServices.Single() == "sample.data"
                          && legacyManifest.Routes.Single().Children.Single().Component == "Sample.Pages.Settings"
                          && legacyManifest.Dependencies.Single().Key == "sample.backend"
                          && roundTrip
                          && !v2Empty.IsAccepted
                          && v2Empty.SchemaVersion == 2
                          && v2Empty.RefusedDeclarations.Contains("installationUnit.requiredCapabilities")
                          && v2Empty.Message.Contains("schema version 2", StringComparison.Ordinal)
                          && !v2Cli.IsAccepted
                          && v2Cli.SchemaVersion == 2
                          && v2Cli.RefusedDeclarations.Contains("installationUnit.requiredCapabilities[cli]")
                          && v2Cli.Message.Contains("schema version 2", StringComparison.Ordinal)
                          && !v3Assets.IsAccepted
                          && v3Assets.SchemaVersion == 3
                          && v3Assets.RefusedDeclarations.Contains("installationUnit.requiredCapabilities")
                          && v3Assets.Message.Contains("schema version 3", StringComparison.Ordinal)
                          && assembly.GetName().Name == "LowlandTech.Core.Contracts"
                          && version.StartsWith("2026.10.4", StringComparison.Ordinal);
        Record("ConsumersUseOneVersionedContract", oneContract,
            $"The contract assembly is {assembly.GetName().Name} at {version} and the version 1 manifest round trips.");

        // The published package that carries the portable declaration is the one
        // a consumer restores.
        var published = FindPublishedPackage();
        var publishedOk = published is not null
                          && published.Dependencies.Count == 0
                          && published.Names.Contains("PortablePluginManifestValidator")
                          && published.Names.Contains("PortablePluginManifestWriter")
                          && published.Names.Contains("PortablePluginManifestDeclaration")
                          && published.Names.OrderBy(name => name, StringComparer.Ordinal).SequenceEqual(
                              PublicTypeNames(assembly).OrderBy(name => name, StringComparer.Ordinal))
                          && new[] { "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "NuGet", "NuGet.Protocol", "LowlandTech.Core", "LowlandTech.Plugins" }
                              .All(forbidden => !published.References.Contains(forbidden))
                          && PortablePluginContentHash.IsSupported("sha256")
                          && !PortablePluginContentHash.IsSupported("md5")
                          && PortablePluginContentHash.Compute("sha256", "content"u8).Digest
                              == PortablePluginContentHash.Compute("sha256", "content"u8).Digest;
        var refused = PortablePluginManifestValidator.Read("""
            {
              "schemaVersion": 2, "key": "sample.hash", "name": "Sample Hash",
              "description": "The manifest that declares an unsupported hash algorithm",
              "target": { "hostTarget": "backend", "version": 2, "extension": "backendApi" },
              "dependencies": [], "routes": [], "slots": [], "placements": [],
              "contributions": { "capabilities": ["sample.hash"] },
              "installationUnit": { "requiredCapabilities": ["sample"], "requiredContracts": {} },
              "definitionFiles": [{ "path": "definitions/backend-api.json", "contentHash": { "algorithm": "md5", "digest": "0" } }]
            }
            """);
        var packageValidates = publishedOk
                               && !refused.IsAccepted
                               && refused.RefusedDeclarations.Contains("definitionFiles");
        Record("PackageOnlyConsumerValidatesDeclarations", packageValidates,
            published is null
                ? "The portable declaration package is neither staged nor published."
                : $"The published package {published.FilePath} carries the portable declaration.");

        // The contract carries no EF Core, no ASP.NET Core, no NuGet runtime, no
        // database driver and no host activation dependency.
        var references = AssemblyReferences(assembly);
        var forbidden = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Relational",
            "Microsoft.AspNetCore",
            "Microsoft.AspNetCore.Components",
            "Microsoft.Data",
            "Microsoft.Data.SqlClient",
            "Npgsql",
            "NuGet",
            "NuGet.Protocol",
            "NuGet.Packaging",
            "System.Data.Sql",
            "LowlandTech.Core",
            "LowlandTech.Plugins"
        };
        var noHost = forbidden.All(name => !references.Contains(name))
                     && references.Contains("System.Runtime")
                     && references.Contains("System.Text.Json");
        Record("ContractHasNoHostOrEfDependencies", noHost,
            $"The contract references {references.Count} assemblies and none of the host runtime ones.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "PortableDeclarationsCarryNoRuntimeTypeNames": PortableDeclarationsCarryNoRuntimeTypeNames = outcome; break;
            case "ConsumersUseOneVersionedContract": ConsumersUseOneVersionedContract = outcome; break;
            case "PackageOnlyConsumerValidatesDeclarations": PackageOnlyConsumerValidatesDeclarations = outcome; break;
            case "ContractHasNoHostOrEfDependencies": ContractHasNoHostOrEfDependencies = outcome; break;
        }
    }

    private static string ReadFixture(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", FixtureFolder, name);
        if (!File.Exists(path)) throw new FileNotFoundException($"The manifest fixture '{name}' is missing.", path);
        return File.ReadAllText(path);
    }

    private static string InformationalVersion(Assembly assembly)
        => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

    /// <summary>The local feed the repository resolves its packages from.</summary>
    private static string? LocalFeed()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "NuGet.config")))
            {
                var candidate = Path.GetFullPath(Path.Combine(directory.FullName, "..", "packages"));
                return Directory.Exists(candidate) ? candidate : null;
            }
            directory = directory.Parent;
        }
        return null;
    }

    /// <summary>The root of the repository the test runs from.</summary>
    private static string? RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "NuGet.config"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "test")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return null;
    }

    /// <summary>The package version the contract assembly of this build carries.</summary>
    private static string? PackageVersionOf(Assembly assembly)
    {
        var informational = InformationalVersion(assembly);
        var plus = informational.IndexOf('+', StringComparison.Ordinal);
        var version = plus < 0 ? informational : informational[..plus];
        var dash = version.LastIndexOf('-');
        return dash < 0 ? null : version;
    }

    /// <summary>
    /// The contract package that carries the portable declaration: the staged
    /// candidate of this release when it exists, and the published package of
    /// the local feed otherwise.
    /// </summary>
    private static PublishedPackage? FindPublishedPackage()
    {
        var root = RepositoryRoot();
        if (root is not null)
        {
            var version = PackageVersionOf(typeof(PortablePluginManifestValidator).Assembly);
            if (version is not null)
            {
                var staged = Path.Combine(root, "artifacts", "local-packages", version);
                if (Directory.Exists(staged))
                {
                    var candidate = Directory.EnumerateFiles(staged, "LowlandTech.Core.Contracts.*.nupkg").FirstOrDefault();
                    var package = candidate is null ? null : PublishedPackage.Read(candidate);
                    if (package is not null) return package;
                }
            }
        }
        var feed = LocalFeed();
        if (feed is null) return null;
        foreach (var path in Directory.EnumerateFiles(feed, "LowlandTech.Core.Contracts.*.nupkg"))
        {
            var package = PublishedPackage.Read(path);
            if (package is not null && package.Names.Contains("PortablePluginManifestDeclaration")) return package;
        }
        return null;
    }

    private static IReadOnlyList<string> AssemblyReferences(Assembly assembly)
    {
        using var stream = File.OpenRead(assembly.Location);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        return metadata.AssemblyReferences
            .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
            .ToArray();
    }

    private static IReadOnlyList<string> PublicTypeNames(Assembly assembly)
    {
        using var stream = File.OpenRead(assembly.Location);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var names = new List<string>();
        foreach (var handle in metadata.TypeDefinitions)
        {
            var type = metadata.GetTypeDefinition(handle);
            var attributes = type.Attributes;
            var visible = (attributes & TypeAttributes.VisibilityMask) is TypeAttributes.Public
                or TypeAttributes.NestedPublic;
            if (visible) names.Add(metadata.GetString(type.Name));
        }
        return names;
    }

    /// <summary>A published contract package of the local feed.</summary>
    private sealed record PublishedPackage(
        string FilePath,
        IReadOnlyList<string> Names,
        IReadOnlyList<string> References,
        IReadOnlyList<string> Dependencies)
    {
        /// <summary>Reads a published package from the local feed.</summary>
        public static PublishedPackage? Read(string path)
        {
            using var archive = ZipFile.OpenRead(path);
            var assembly = archive.Entries.FirstOrDefault(entry => entry.FullName.StartsWith("lib/", StringComparison.Ordinal)
                && entry.FullName.EndsWith(".dll", StringComparison.Ordinal));
            var nuspec = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
            if (assembly is null || nuspec is null) return null;

            var extracted = Path.Combine(Path.GetTempPath(), "core-contract-probe-" + Guid.NewGuid().ToString("N") + ".dll");
            try
            {
                assembly.ExtractToFile(extracted, overwrite: true);
                var names = new List<string>();
                var references = new List<string>();
                using (var stream = File.OpenRead(extracted))
                using (var pe = new PEReader(stream))
                {
                    var metadata = pe.GetMetadataReader();
                    foreach (var handle in metadata.TypeDefinitions)
                    {
                        var type = metadata.GetTypeDefinition(handle);
                        var attributes = type.Attributes;
                        var visible = (attributes & TypeAttributes.VisibilityMask) is TypeAttributes.Public
                            or TypeAttributes.NestedPublic;
                        if (visible) names.Add(metadata.GetString(type.Name));
                    }
                    foreach (var handle in metadata.AssemblyReferences)
                        references.Add(metadata.GetString(metadata.GetAssemblyReference(handle).Name));
                }

                var dependencies = new List<string>();
                using (var reader = new StreamReader(nuspec.Open()))
                {
                    var document = System.Xml.Linq.XDocument.Parse(reader.ReadToEnd());
                    dependencies.AddRange(document.Descendants()
                        .Where(element => element.Name.LocalName == "dependency")
                        .Select(element => (string?)element.Attribute("id") ?? string.Empty)
                        .Where(id => id.Length > 0));
                }
                return new(path, names, references, dependencies);
            }
            finally
            {
                if (File.Exists(extracted)) File.Delete(extracted);
            }
        }
    }
}
