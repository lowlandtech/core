using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using LowlandTech.Core.Contracts;
using LowlandTech.Core.Contracts.Plugins;
using LowlandTech.Core.Contracts.Transitions;

namespace LowlandTech.Core.Tests;

/// <summary>
/// The acceptance inventory of CORE0002-US01-SC01. The fixtures are the exact
/// serialized Backend transition definitions that the Backend shipped at
/// commit 74ce46f, so every Fact here observes the real host documents and not
/// a shape invented by this lane.
/// </summary>
public sealed class GraphContractAcceptanceTests
{
    private const string FixtureFolder = "BKPL0002.US01.SharedContracts";

    private static readonly string[] FixtureNames =
    [
        "auth-create-user.json",
        "builder-dummy-prepare.json",
        "harness-has-container.json",
        "harness-has-layout.json",
        "harness-list-main-intents.json"
    ];

    [Fact]
    public void UAC01_BackendTransitionDocumentRoundTrips()
    {
        foreach (var name in FixtureNames)
        {
            var document = ReadFixtureDocument(name);
            var contract = TransitionContractDocument.Read(document.ToJsonString());

            contract.Schema.ShouldBe(TransitionContractDocument.SupportedSchema, name);
            contract.Kind.ShouldBe(TransitionContractDocument.SupportedKind, name);
            contract.Plugin.ShouldNotBeNullOrWhiteSpace(name);
            contract.Key.ShouldNotBeNullOrWhiteSpace(name);
            contract.Capability.ShouldNotBeNullOrWhiteSpace(name);
            contract.Aspects.ShouldNotBeNull(name);

            // The canonical form of the contract has to be the exact document the
            // host wrote, member order included. A byte comparison proves both
            // the semantics and the wire.
            var roundTripped = TransitionContractDocument.Write(contract);
            roundTripped.ShouldBe(document.ToJsonString(), name);

            // The rules of the contract accept every document the host accepts.
            TransitionContractValidator.Validate(contract).ShouldBeEmpty(name);
        }
    }

    [Fact]
    public void UAC02_ConsumersUseOneContractPackage()
    {
        var assembly = typeof(TransitionDefinitionDocument).Assembly;
        var identity = assembly.GetName();

        // One package identity, one assembly identity and one version line. The
        // contract never reuses the compatibility package identity.
        identity.Name.ShouldBe("LowlandTech.Core.Contracts");
        identity.Name.ShouldNotBe("LowlandTech.Core");
        InformationalVersion(assembly).ShouldStartWith("2026.10.4");

        // A package-only consumer reaches the contract through this assembly
        // alone. The assembly carries no reference to the compatibility package,
        // so the resolved closure of every existing Core consumer is unchanged
        // and no consumer can end up with two copies of one contract type.
        var references = AssemblyReferences(assembly);
        references.ShouldNotContain("LowlandTech.Core");
        references.ShouldNotContain("LowlandTech.Components.Core");

        // The shared types carry no name that the compatibility package already
        // publishes for a different meaning.
        var published = PublicTypeNames(assembly);
        foreach (var legacy in new[] { "Node", "Edge", "NodeType", "EdgeType", "Property", "PropertyDefinition", "Rule", "RuleGroup", "Commit", "Document", "PluginDescriptor" })
            published.ShouldNotContain(legacy);

        // The document is reachable through the public contract surface, so a
        // package-only consumer can read and validate it without a host type.
        var document = ReadFixtureDocument("harness-list-main-intents.json");
        var contract = TransitionContractDocument.Read(document.ToJsonString());
        contract.Aspects.UseCase.Cases.Single().Outcome.ShouldBe("Succeeded");
        TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract,
            contract.Plugin,
            new PluginScope("harness.local", null, null, null, null),
            new PluginScope("harness.local", null, null, null, null),
            [contract.Capability])).ShouldBeEmpty();
    }

    [Fact]
    public void UAC03_InvalidDefinitionsReceiveNamedRefusals()
    {
        var document = ReadFixtureDocument("harness-list-main-intents.json");
        var contract = TransitionContractDocument.Read(document.ToJsonString());
        var scope = new PluginScope("harness.local", null, null, null, null);
        var capabilities = new[] { "component.list-intents" };

        // Wrong owner.
        var wrongOwner = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, "Auth", scope, scope, capabilities));
        wrongOwner.ShouldContain(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.OwnerMismatch);
        wrongOwner.ShouldContain(diagnostic => diagnostic.Member == "plugin");

        // Wrong schema.
        var wrongSchema = TransitionContractValidator.Validate(contract with { Schema = "gs.transition/v2" });
        wrongSchema.ShouldContain(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.SchemaUnsupported);
        wrongSchema.ShouldContain(diagnostic => diagnostic.Message.Contains("gs.transition/v1"));

        // Wrong scope.
        var wrongScope = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, new PluginScope("other.host", null, null, null, null), capabilities));
        wrongScope.ShouldContain(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.ScopeInvalid);

        // Unknown capability reference.
        var unknownCapability = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract with { Capability = "harness.unknown-capability" }, contract.Plugin, scope, scope, capabilities));
        unknownCapability.ShouldContain(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.CapabilityUnknown);
        unknownCapability.ShouldContain(diagnostic => diagnostic.Message.Contains("is not registered"));

        // Wrong content hash.
        var canonical = TransitionContractDocument.Canonicalize(contract);
        var wrongHash = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, scope, capabilities,
            new PluginContentHash("sha256", new string('0', 64))));
        wrongHash.ShouldContain(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.HashMismatch);

        // The same content hashes to the digest the contract computes.
        var correctHash = PluginContentHash.Compute("sha256", canonical);
        TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, scope, capabilities, correctHash)).ShouldBeEmpty();

        // An unsupported hash algorithm is refused by name too.
        TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, scope, capabilities, new PluginContentHash("md5", "0")))
            .ShouldContain(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.HashAlgorithmUnknown);
    }

    [Fact]
    public void UAC04_PortablePackageHasNoHostDependencies()
    {
        var assembly = typeof(TransitionDefinitionDocument).Assembly;
        var references = AssemblyReferences(assembly);

        // The contract carries no EF Core, no ASP.NET Core, no database driver
        // and no host runtime dependency.
        foreach (var forbidden in new[] { "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Relational", "Microsoft.AspNetCore", "Microsoft.AspNetCore.Components", "Microsoft.Data", "Microsoft.Data.SqlClient", "Npgsql", "LowlandTech.Core" })
            references.ShouldNotContain(forbidden);

        // The contract reaches the framework only through the data contracts of
        // the base class library.
        references.ShouldContain("System.Runtime");
        references.ShouldContain("System.Text.Json");
    }

    [Fact]
    public void UAC05_ExistingCorePackageRemainsAvailable()
    {
        var assembly = typeof(TransitionDefinitionDocument).Assembly;

        // The contract is additive: it does not depend on the compatibility
        // package, so the consumers of LowlandTech.Core keep their closure and
        // their pins.
        AssemblyReferences(assembly).ShouldNotContain("LowlandTech.Core");

        // The contract has its own package identity and its own version line, so
        // no existing package is replaced by it.
        var contractVersion = InformationalVersion(assembly);
        contractVersion.ShouldStartWith("2026.10.4");
        contractVersion.ShouldNotStartWith("2026.10.3");

        var coreAssembly = typeof(NodeTypeRegistry).Assembly;
        coreAssembly.GetName().Name.ShouldBe("LowlandTech.Core");
        InformationalVersion(coreAssembly).ShouldStartWith("2026.10.3");
    }

    private static string InformationalVersion(Assembly assembly)
        => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

    /// <summary>
    /// The assemblies the given assembly references. The metadata is read
    /// directly, so the observation needs no reflection call that a trimmed
    /// build would refuse.
    /// </summary>
    private static IReadOnlyList<string> AssemblyReferences(Assembly assembly)
    {
        using var stream = File.OpenRead(assembly.Location);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        return metadata.AssemblyReferences
            .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
            .ToArray();
    }

    /// <summary>The public type names the given assembly publishes.</summary>
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

    private static JsonObject ReadFixtureDocument(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", FixtureFolder, name);
        File.Exists(path).ShouldBeTrue($"The Backend parity fixture '{name}' is missing from the test output.");
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        root["fixture"]!.GetValue<string>().ShouldBe(Path.GetFileNameWithoutExtension(name));
        return root["document"]!.AsObject();
    }
}
