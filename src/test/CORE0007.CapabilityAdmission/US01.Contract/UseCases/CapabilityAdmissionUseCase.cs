using System.Text.Json.Nodes;
using LowlandTech.Core.Contracts.Plugins;
using LowlandTech.Core.Contracts.Transitions;

namespace LowlandTech.Core.Tests.CORE0007.CapabilityAdmission.US01.Contract.UseCases;

/// <summary>
/// The acceptance inventory of CORE0007-US01-SC01. The fixture is the exact
/// serialized Backend transition definition that the Backend shipped at commit
/// 74ce46f, so the observations are of a real host document and not of a shape
/// invented by this lane.
///
/// Every observation is captured here so that every Fact of the scenario
/// asserts one of them against the production contract.
/// </summary>
public sealed class CapabilityAdmissionUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    private const string FixtureFolder = "BKPL0002.US01.SharedContracts";
    private const string FixtureName = "harness-list-main-intents.json";

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>An empty registration refuses the declared capability with the named refusal.</summary>
    public bool? EmptyRegistrationRefusesTheDeclaredCapability { get; private set; }

    /// <summary>An absent registration leaves the declared capability unconstrained.</summary>
    public bool? AbsentRegistrationLeavesTheCapabilityUnconstrained { get; private set; }

    /// <summary>The canonical bytes of the document are unchanged.</summary>
    public bool? TheCanonicalBytesAreUnchanged { get; private set; }

    /// <summary>A capability that the host registered is admitted.</summary>
    public bool? ARegisteredCapabilityIsAdmitted { get; private set; }

    /// <summary>The unchanged refusals of the v1 contract still hold.</summary>
    public bool? TheUnchangedRefusalsStillHold { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        var canonical = ReadFixtureDocument();
        var contract = TransitionContractDocument.Read(canonical);
        var scope = new PortablePluginScope("harness.local", null, null, null, null);

        // A host that registered no capability at all refuses the declared
        // capability, because nothing is registered.
        var empty = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, scope, Array.Empty<string>()));
        var refused = empty.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.CapabilityUnknown);
        Record("EmptyRegistrationRefusesTheDeclaredCapability", refused,
            refused
                ? $"The empty registration refused '{contract.Capability}' with {TransitionContractDiagnosticCodes.CapabilityUnknown}."
                : $"The empty registration refused nothing: {Describe(empty)}");

        // A host that passes no registration at all declines to constrain the
        // capability, so the contract has nothing to compare against.
        var absent = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, scope, null));
        var unconstrained = absent.Count == 0;
        Record("AbsentRegistrationLeavesTheCapabilityUnconstrained", unconstrained,
            unconstrained
                ? "The absent registration left the declared capability unconstrained."
                : $"The absent registration refused: {Describe(absent)}");

        // The rule decides the admission of a capability only, so the canonical
        // bytes of the document stay exactly the bytes of the host document.
        var unchanged = TransitionContractDocument.Write(contract) == canonical
                        && TransitionContractDocument.Canonicalize(contract) == canonical;
        Record("TheCanonicalBytesAreUnchanged", unchanged,
            unchanged
                ? $"The canonical form of the document is still the {canonical.Length} bytes of the host document."
                : "The canonical form of the document differs from the host document.");

        // A capability that the host registered is admitted as before.
        var registered = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, scope, new[] { contract.Capability }));
        var admitted = registered.Count == 0;
        Record("ARegisteredCapabilityIsAdmitted", admitted,
            admitted
                ? $"The registered capability '{contract.Capability}' was admitted."
                : $"The registered capability was refused: {Describe(registered)}");

        // The other refusals of the v1 contract are untouched by the rule.
        var wrongSchema = TransitionContractValidator.Validate(contract with { Schema = "gs.transition/v2" });
        var wrongOwner = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, "Auth", scope, scope, new[] { contract.Capability }));
        var wrongScope = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, new PortablePluginScope("other.host", null, null, null, null), new[] { contract.Capability }));
        var wrongHash = TransitionContractValidator.Admit(new TransitionAdmissionRequest(
            contract, contract.Plugin, scope, scope, new[] { contract.Capability },
            new PortablePluginContentHash("sha256", new string('0', 64))));
        var held = wrongSchema.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.SchemaUnsupported)
                  && wrongOwner.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.OwnerMismatch)
                  && wrongScope.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.ScopeInvalid)
                  && wrongHash.Any(diagnostic => diagnostic.Code == TransitionContractDiagnosticCodes.HashMismatch);
        Record("TheUnchangedRefusalsStillHold", held,
            held
                ? "The schema, the owner, the scope and the content hash refusals still hold."
                : $"The refusals that no longer hold: {Describe([.. wrongSchema, .. wrongOwner, .. wrongScope, .. wrongHash])}");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "EmptyRegistrationRefusesTheDeclaredCapability":
                EmptyRegistrationRefusesTheDeclaredCapability = outcome;
                break;
            case "AbsentRegistrationLeavesTheCapabilityUnconstrained":
                AbsentRegistrationLeavesTheCapabilityUnconstrained = outcome;
                break;
            case "TheCanonicalBytesAreUnchanged":
                TheCanonicalBytesAreUnchanged = outcome;
                break;
            case "ARegisteredCapabilityIsAdmitted":
                ARegisteredCapabilityIsAdmitted = outcome;
                break;
            case "TheUnchangedRefusalsStillHold":
                TheUnchangedRefusalsStillHold = outcome;
                break;
        }
    }

    private static string Describe(IReadOnlyList<TransitionContractDiagnostic> diagnostics)
        => diagnostics.Count == 0 ? "nothing was refused" : string.Join("; ", diagnostics);

    private static string ReadFixtureDocument()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "fixtures", FixtureFolder, FixtureName);
        if (!File.Exists(path)) throw new FileNotFoundException($"The Backend parity fixture '{FixtureName}' is missing.", path);
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        if (root["fixture"]!.GetValue<string>() != Path.GetFileNameWithoutExtension(FixtureName))
            throw new InvalidOperationException($"The fixture '{FixtureName}' does not carry its own name.");
        return root["document"]!.AsObject().ToJsonString();
    }
}
