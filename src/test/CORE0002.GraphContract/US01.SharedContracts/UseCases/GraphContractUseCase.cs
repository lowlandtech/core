using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json.Nodes;
using LowlandTech.Core.Contracts;
using LowlandTech.Core.Contracts.Plugins;
using LowlandTech.Core.Contracts.Transitions;

namespace LowlandTech.Core.Tests.CORE0002.GraphContract.US01.SharedContracts.UseCases;

/// <summary>
/// The acceptance inventory of CORE0002-US01-SC01. The fixtures are the exact
/// serialized Backend transition definitions that the Backend shipped at
/// commit 74ce46f, so the observations here are of the real host documents and
/// not of a shape invented by this lane.
///
/// Every observation is captured here so that every Fact of the scenario
/// asserts one of them against the production contract.
/// </summary>
public sealed class GraphContractUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
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

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>Every host document round trips to the exact document it was.</summary>
    public bool? DocumentsRoundTrip { get; private set; }

    /// <summary>The contract carries one package identity and no colliding name.</summary>
    public bool? ContractHasOnePackageIdentity { get; private set; }

    /// <summary>The portable family of the contract is complete.</summary>
    public bool? PortableFamilyIsComplete { get; private set; }

    /// <summary>An invalid definition receives the named refusal.</summary>
    public bool? InvalidDefinitionsReceiveNamedRefusals { get; private set; }

    /// <summary>The contract carries no host dependency.</summary>
    public bool? ContractHasNoHostDependencies { get; private set; }

    /// <summary>The existing Core package stays available next to the contract.</summary>
    public bool? ExistingCorePackageRemainsAvailable { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        // Every document that the host writes is read back unchanged and the
        // rules of the contract accept it.
        var roundTrip = true;
        var details = new List<string>();
        foreach (var name in FixtureNames)
        {
            var document = ReadFixtureDocument(name);
            var contract = TransitionContractDocument.Read(document.ToJsonString());
            var same = contract.Schema == TransitionContractDocument.SupportedSchema
                       && contract.Kind == TransitionContractDocument.SupportedKind
                       && !string.IsNullOrWhiteSpace(contract.Plugin)
                       && !string.IsNullOrWhiteSpace(contract.Key)
                       && !string.IsNullOrWhiteSpace(contract.Capability)
                       && contract.Aspects is not null
                       && TransitionContractDocument.Write(contract) == document.ToJsonString()
                       && TransitionContractValidator.Validate(contract).Count == 0;
            if (!same) details.Add(name);
            roundTrip &= same;
        }
        Record("DocumentsRoundTrip", roundTrip,
            roundTrip
                ? $"{FixtureNames.Length} host documents round trip to the exact document they were."
                : $"The documents {string.Join(", ", details)} did not round trip.");

        var assembly = typeof(TransitionDefinitionDocument).Assembly;
        var identity = assembly.GetName().Name;
        var version = InformationalVersion(assembly);
        var published = PublicTypeNames(assembly);
        var references = AssemblyReferences(assembly);
        var reserved = new[]
        {
            "Node", "Edge", "NodeType", "EdgeType", "Property", "PropertyDefinition", "Rule", "RuleGroup",
            "Commit", "Document", "PluginDescriptor", "PluginKey", "PluginRevision", "PluginScope",
            "PluginImplementationKind", "PluginLifetime", "PluginImplementationBinding", "PluginProvenance",
            "ContentHash", "PluginManifest", "PluginContributions", "PluginTarget", "PluginInstallationUnit",
            "PluginDependency", "PluginRoute", "PluginSlot", "PluginComponentPlacement", "PluginClientAsset",
            "PluginClientAssets", "PluginPackageDescriptor", "PluginContentHash", "PluginIdentity"
        };
        var oneIdentity = identity == "LowlandTech.Core.Contracts"
                         && version.StartsWith("2026.10.4", StringComparison.Ordinal)
                         && !references.Contains("LowlandTech.Core")
                         && !references.Contains("LowlandTech.Components.Core")
                         && reserved.All(name => !published.Contains(name));
        Record("ContractHasOnePackageIdentity", oneIdentity,
            $"The contract assembly is {identity} at {version} and publishes no reserved name.");

        var portable = new[]
        {
            "PortablePluginKey", "PortablePluginContentHash", "PortablePluginRevision", "PortablePluginScope",
            "PortablePluginImplementationKind", "PortablePluginLifetime", "PortablePluginImplementationBinding",
            "PortablePluginProvenance", "PortablePluginIdentity"
        };
        var complete = portable.All(published.Contains);
        Record("PortableFamilyIsComplete", complete,
            $"{portable.Count(name => published.Contains(name))} of {portable.Length} portable names are published.");

        var intentDocument = ReadFixtureDocument("harness-list-main-intents.json");
        var intentContract = TransitionContractDocument.Read(intentDocument.ToJsonString());
        var scope = new PortablePluginScope("harness.local", null, null, null, null);
        var capabilities = new[] { "component.list-intents" };
        var wrongOwner = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            intentContract, "Auth", scope, scope, capabilities));
        var wrongSchema = TransitionContractValidator.Validate(intentContract with { Schema = "gs.transition/v2" });
        var wrongScope = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            intentContract, intentContract.Plugin, scope, new PortablePluginScope("other.host", null, null, null, null), capabilities));
        var unknownCapability = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            intentContract with { Capability = "harness.unknown-capability" }, intentContract.Plugin, scope, scope, capabilities));
        var wrongHash = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            intentContract, intentContract.Plugin, scope, scope, capabilities,
            new PortablePluginContentHash("sha256", new string('0', 64))));
        var correctHash = PortablePluginContentHash.Compute("sha256", TransitionContractDocument.Canonicalize(intentContract));
        var admitted = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            intentContract, intentContract.Plugin, scope, scope, capabilities, correctHash));
        var wrongAlgorithm = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            intentContract, intentContract.Plugin, scope, scope, capabilities, new PortablePluginContentHash("md5", "0")));
        var refused = wrongOwner.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.OwnerMismatch)
                     && wrongSchema.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.SchemaUnsupported)
                     && wrongScope.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.ScopeInvalid)
                     && unknownCapability.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.CapabilityUnknown)
                     && wrongHash.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.HashMismatch)
                     && wrongAlgorithm.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.HashAlgorithmUnknown)
                     && admitted.Count == 0;
        Record("InvalidDefinitionsReceiveNamedRefusals", refused,
            refused
                ? "The invalid definitions received their named refusals and the correct hash was admitted."
                : "The invalid definitions did not receive the named refusals.");

        var forbidden = new[]
        {
            "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Relational", "Microsoft.AspNetCore",
            "Microsoft.AspNetCore.Components", "Microsoft.Data", "Microsoft.Data.SqlClient", "Npgsql",
            "LowlandTech.Core"
        };
        var noHost = forbidden.All(name => !references.Contains(name))
                     && references.Contains("System.Runtime")
                     && references.Contains("System.Text.Json");
        Record("ContractHasNoHostDependencies", noHost,
            $"The contract references {references.Count} assemblies and none of the host runtime ones.");

        var coreAssembly = typeof(NodeTypeRegistry).Assembly;
        var coreVersion = InformationalVersion(coreAssembly);
        var available = !references.Contains("LowlandTech.Core")
                        && version.StartsWith("2026.10.4", StringComparison.Ordinal)
                        && !version.StartsWith("2026.10.3", StringComparison.Ordinal)
                        && coreAssembly.GetName().Name == "LowlandTech.Core"
                        && coreVersion.StartsWith("2026.10.3", StringComparison.Ordinal);
        Record("ExistingCorePackageRemainsAvailable", available,
            $"The contract is at {version} and the Core package stays at {coreVersion}.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "DocumentsRoundTrip": DocumentsRoundTrip = outcome; break;
            case "ContractHasOnePackageIdentity": ContractHasOnePackageIdentity = outcome; break;
            case "PortableFamilyIsComplete": PortableFamilyIsComplete = outcome; break;
            case "InvalidDefinitionsReceiveNamedRefusals": InvalidDefinitionsReceiveNamedRefusals = outcome; break;
            case "ContractHasNoHostDependencies": ContractHasNoHostDependencies = outcome; break;
            case "ExistingCorePackageRemainsAvailable": ExistingCorePackageRemainsAvailable = outcome; break;
        }
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
        if (!File.Exists(path)) throw new FileNotFoundException($"The Backend parity fixture '{name}' is missing.", path);
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var expected = Path.GetFileNameWithoutExtension(name);
        if (root["fixture"]!.GetValue<string>() != expected)
            throw new InvalidOperationException($"The fixture '{name}' does not carry its own name.");
        return root["document"]!.AsObject();
    }
}
