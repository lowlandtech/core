using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Nodes;
using LowlandTech.Core.Contracts.Plugins;

namespace LowlandTech.Core.Tests;

/// <summary>
/// The acceptance inventory of CORE0002-US02-SC01. The fixtures are the exact
/// manifest documents the Plugins lane reported: the version 1 manifest of a
/// published package and the three version 2 and version 3 shapes its writer
/// accepts and its inspector refuses. Every Fact here observes the real
/// documents and not a shape invented by this lane.
/// </summary>
public sealed class PluginContributionContractAcceptanceTests
{
    private const string FixtureFolder = "CORE0002.US02.SharedPlugin";

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "The fact reads the public properties of its own contract types to prove that none of them carries System.Type. The test assembly is not trimmed and the contract assembly it reads is.")]
    public void UAC01_PortableDeclarationsContainNoRuntimeTypeNames()
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
        read.IsAccepted.ShouldBeTrue(read.Message);
        var manifest = read.Manifest!;

        // The target is versioned and carries the extension. The extension is
        // not a new member of the target enum, so a host that predates it keeps
        // reading the plain meaning of the same target.
        manifest.Target.Target.ShouldBe(PortablePluginTarget.Backend);
        manifest.Target.Version.ShouldBe(2);
        manifest.Target.Extension.ShouldBe(PortablePluginTargetExtension.BackendApi);
        PortablePluginTargetDeclaration.PlainBackend.Target.ShouldBe(PortablePluginTarget.Backend);
        PortablePluginTargetDeclaration.PlainBackend.Extension.ShouldBe(PortablePluginTargetExtension.None);

        // The definition file carries its content hash as data.
        manifest.DefinitionFiles.Count.ShouldBe(1);
        manifest.DefinitionFiles.Single().Path.ShouldBe("definitions/backend-api.json");
        manifest.DefinitionFiles.Single().ContentHash.Algorithm.ShouldBe(hash.Algorithm);
        manifest.DefinitionFiles.Single().ContentHash.Digest.ShouldBe(hash.Digest);

        // A contribution that names a type that does not exist is still read.
        // The contract resolves nothing it reads, so an untrusted document can
        // never make it load a type.
        manifest.Contributions.Capabilities.Single().ShouldBe("Sample.Components.DoesNotExist");
        manifest.Contributions.DbContexts.Single().ShouldBe("Sample.Data.DoesNotExist");

        // No member of the declaration carries a type, so the declaration
        // cannot hold an executable type that untrusted data named. The public
        // properties of the declaration types are read to prove it; the test
        // assembly is not trimmed and the contract assembly is.
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
                property.PropertyType.ShouldNotBe(typeof(Type));
        }

        // The frontend meaning of a target is versioned the same way.
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
        frontendRead.IsAccepted.ShouldBeTrue(frontendRead.Message);
        frontendRead.Manifest!.Target.Target.ShouldBe(PortablePluginTarget.Blazor);
        frontendRead.Manifest!.Target.Version.ShouldBe(2);
        frontendRead.Manifest!.Target.Extension.ShouldBe(PortablePluginTargetExtension.Frontend);
    }

    [Fact]
    public void UAC02_ConsumersUseOneVersionedContract()
    {
        // The manifest of a published version 1 package stays readable.
        var read = PortablePluginManifestValidator.Read(ReadFixture("manifest-v1.json"));
        read.IsAccepted.ShouldBeTrue(read.Message);
        var manifest = read.Manifest!;
        manifest.SchemaVersion.ShouldBe(1);
        manifest.Key.ShouldBe("sample.ui");
        manifest.Name.ShouldBe("Sample UI");
        manifest.Target.Target.ShouldBe(PortablePluginTarget.Blazor);
        manifest.Target.Version.ShouldBe(PortablePluginTargetDeclaration.LegacyVersion);
        manifest.Contributions.Components.Single().ShouldBe("Sample.Components.Details");
        manifest.Contributions.DataServices.Single().ShouldBe("sample.data");
        manifest.Routes.Single().Children.Single().Component.ShouldBe("Sample.Pages.Settings");
        manifest.Dependencies.Single().Key.ShouldBe("sample.backend");

        // The declaration writes a document the contract reads back unchanged.
        var written = PortablePluginManifestWriter.Write(manifest);
        var reread = PortablePluginManifestValidator.Read(written);
        reread.IsAccepted.ShouldBeTrue(reread.Message);
        var rewritten = PortablePluginManifestWriter.Write(reread.Manifest!);
        JsonNode.DeepEquals(JsonNode.Parse(written), JsonNode.Parse(rewritten)).ShouldBeTrue();

        // The version 2 and version 3 shapes the Plugins writer accepts and its
        // inspector refuses receive the named versioned refusal of the
        // contract, so a consumer knows which version to move to.
        var v2Empty = PortablePluginManifestValidator.Read(ReadFixture("manifest-v2-empty.json"));
        v2Empty.IsAccepted.ShouldBeFalse();
        v2Empty.SchemaVersion.ShouldBe(2);
        v2Empty.RefusedDeclarations.ShouldContain("installationUnit.requiredCapabilities");
        v2Empty.Message.ShouldContain("schema version 2");

        var v2Cli = PortablePluginManifestValidator.Read(ReadFixture("manifest-v2-cli.json"));
        v2Cli.IsAccepted.ShouldBeFalse();
        v2Cli.SchemaVersion.ShouldBe(2);
        v2Cli.RefusedDeclarations.ShouldContain("installationUnit.requiredCapabilities[cli]");
        v2Cli.Message.ShouldContain("schema version 2");

        var v3Assets = PortablePluginManifestValidator.Read(ReadFixture("manifest-v3-assets.json"));
        v3Assets.IsAccepted.ShouldBeFalse();
        v3Assets.SchemaVersion.ShouldBe(3);
        v3Assets.RefusedDeclarations.ShouldContain("installationUnit.requiredCapabilities");
        v3Assets.Message.ShouldContain("schema version 3");

        // One contract package identity and one version line.
        var assembly = typeof(PortablePluginManifestValidator).Assembly;
        assembly.GetName().Name.ShouldBe("LowlandTech.Core.Contracts");
        InformationalVersion(assembly).ShouldStartWith("2026.10.4");
    }

    [Fact]
    public void UAC03_PackageOnlyConsumerValidatesDeclarations()
    {
        var published = FindPublishedPackage();
        published.ShouldNotBeNull("The portable declaration package is neither staged nor published.");

        // The published package that carries the portable declaration is the one
        // a consumer restores. It declares no dependency, so the consumer that
        // restores it reaches the contract through the package alone and needs
        // no source checkout.
        published!.Dependencies.ShouldBeEmpty();
        published.Names.ShouldContain("PortablePluginManifestValidator");
        published.Names.ShouldContain("PortablePluginManifestWriter");
        published.Names.ShouldContain("PortablePluginManifestDeclaration");

        // The behavior the facts above prove is the behavior of the published
        // artifact: the published assembly and the built assembly publish the
        // same public surface.
        published.Names.OrderBy(name => name, StringComparer.Ordinal)
            .ShouldBe(PublicTypeNames(typeof(PortablePluginManifestValidator).Assembly)
                .OrderBy(name => name, StringComparer.Ordinal).ToArray());

        // The published assembly reaches the framework only through the data
        // contracts of the base class library, so a package-only consumer needs
        // no host runtime beside it.
        foreach (var forbidden in new[] { "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "NuGet", "NuGet.Protocol", "LowlandTech.Core", "LowlandTech.Plugins" })
            published.References.ShouldNotContain(forbidden);

        // The content hash validation the contract owns rejects a digest that
        // is not a digest of a supported algorithm.
        var unsupported = PortablePluginContentHash.IsSupported("md5");
        unsupported.ShouldBeFalse();
        PortablePluginContentHash.IsSupported("sha256").ShouldBeTrue();
        PortablePluginContentHash.Compute("sha256", "content"u8).Digest
            .ShouldBe(PortablePluginContentHash.Compute("sha256", "content"u8).Digest);

        // A definition file that declares an unsupported hash algorithm is
        // refused by name.
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
        refused.IsAccepted.ShouldBeFalse();
        refused.RefusedDeclarations.ShouldContain("definitionFiles");
    }

    [Fact]
    public void UAC04_ContractHasNoHostOrEfDependencies()
    {
        var assembly = typeof(PortablePluginManifestValidator).Assembly;

        // The contract carries no EF Core, no ASP.NET Core, no NuGet runtime, no
        // database driver and no host activation dependency.
        foreach (var forbidden in new[]
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
        })
            AssemblyReferences(assembly).ShouldNotContain(forbidden);

        // The contract reaches the framework only through the data contracts of
        // the base class library.
        AssemblyReferences(assembly).ShouldContain("System.Runtime");
        AssemblyReferences(assembly).ShouldContain("System.Text.Json");
    }

    private static string ReadFixture(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", FixtureFolder, name);
        File.Exists(path).ShouldBeTrue($"The manifest fixture '{name}' is missing from the test output.");
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
                    dependencies.AddRange(document.Descendants("dependency")
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
