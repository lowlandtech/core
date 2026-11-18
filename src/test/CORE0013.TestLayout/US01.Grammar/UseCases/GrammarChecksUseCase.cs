using System.Text.Json.Nodes;

namespace LowlandTech.Core.Tests.CORE0013.TestLayout.US01.Grammar.UseCases;

/// <summary>
/// The Core owned measurement of the grammar checks that read a trait key
/// through a compile-time constant and require a scenario to declare the area
/// namespace of its path.
///
/// The use case runs the repository owned gate <c>eng/verify-test-layout.py</c>
/// over the repository tree, over a positive alias fixture and over the negative
/// fixtures of the new rules, and records one named result per observation. It
/// proves the tooling, not the behavior of a consuming lane: no external
/// checkout is read.
/// </summary>
public sealed class GrammarChecksUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>A trait key written through a constant alias resolves like the literal.</summary>
    public bool? AliasResolved { get; private set; }

    /// <summary>A repeated alias UAC is rejected with the duplicate rule.</summary>
    public bool? DuplicateRejected { get; private set; }

    /// <summary>A scenario without a namespace is rejected.</summary>
    public bool? NamespaceRequired { get; private set; }

    /// <summary>A namespace that carries another area than the path is rejected.</summary>
    public bool? NamespaceIdentityChecked { get; private set; }

    /// <summary>A trait key that no constant declares is reported.</summary>
    public bool? UnresolvedKeyReported { get; private set; }

    /// <summary>The Core tree keeps no violation and the gate keeps its exit code.</summary>
    public bool? CoreTreeClean { get; private set; }

    /// <summary>How often the fixture arranged this use case.</summary>
    public int ArrangementCount { get; private set; }

    /// <summary>How often the fixture invoked this use case.</summary>
    public int InvocationCount { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    private string? RepositoryRoot { get; set; }

    public Task ArrangeAsync()
    {
        ArrangementCount++;
        RepositoryRoot = global::LowlandTech.Core.Tests.Refinement.GateRunner
            .RepositoryRoot("verify-test-layout.py");
        return Task.CompletedTask;
    }

    public Task InvokeAsync()
    {
        InvocationCount++;
        Measure();
        return Task.CompletedTask;
    }

    private void Measure()
    {
        var root = RepositoryRoot;
        if (root is null)
        {
            foreach (var key in new[]
                     {
                         "AliasResolved", "DuplicateRejected", "NamespaceRequired",
                         "NamespaceIdentityChecked", "UnresolvedKeyReported", "CoreTreeClean"
                     })
            {
                Record(key, false, "The repository root was not found.");
            }
            return;
        }

        var fixtures = Path.Combine(root, "eng", "test-layout", "fixtures");

        var valid = Run(root, Path.Combine(fixtures, "valid", "alias-uac-trait"));
        var validFacts = (valid["scenarios"] as JsonArray ?? []).SelectMany(scenario =>
            (scenario!["facts"] as JsonArray ?? []).Select(fact => fact!["uac"]?.GetValue<string>())).ToList();
        var aliasOk = Violations(valid).Count == 0
                      && (valid["counts"]!["scenarios"]!.GetValue<int>() == 1)
                      && validFacts.Count == 1 && validFacts[0] == "01";
        Record("AliasResolved", aliasOk,
            aliasOk
                ? "The alias fixture is accepted, holds one scenario, and its Fact reads the UAC '01'."
                : $"The alias fixture reported {Violations(valid).Count} violation(s) and {validFacts.Count} entries.");

        var duplicate = Run(root, Path.Combine(fixtures, "invalid", "duplicate-uac-trait"));
        var duplicateCodes = Violations(duplicate).Select(violation => violation["code"]!.GetValue<string>()).ToHashSet();
        var aliasDuplicate = Violations(duplicate)
            .Where(violation => violation["file"]!.GetValue<string>().StartsWith("CORE0013.", StringComparison.Ordinal))
            .ToList();
        var duplicateOk = duplicateCodes.SetEquals(["duplicate-uac-trait"])
                          && aliasDuplicate.Count == 1
                          && aliasDuplicate[0]["detail"]!.GetValue<string>().Contains("already used", StringComparison.Ordinal);
        Record("DuplicateRejected", duplicateOk,
            duplicateOk
                ? "The alias fixture repeats UAC04 and the gate reports the duplicate at the second Fact."
                : $"The duplicate scan reported {string.Join(", ", duplicateCodes)} and {aliasDuplicate.Count} alias finding(s).");

        var missing = Run(root, Path.Combine(fixtures, "invalid", "missing-namespace"));
        var missingOk = Codes(missing).SetEquals(["missing-namespace"]);
        Record("NamespaceRequired", missingOk,
            missingOk
                ? "The scenario without a namespace is rejected with missing-namespace."
                : $"The missing-namespace fixture reported {string.Join(", ", Codes(missing))}.");

        var mismatch = Run(root, Path.Combine(fixtures, "invalid", "namespace-area-mismatch"));
        var mismatchOk = Codes(mismatch).SetEquals(["namespace-area-mismatch"]);
        Record("NamespaceIdentityChecked", mismatchOk,
            mismatchOk
                ? "The namespace naming another area is rejected with namespace-area-mismatch."
                : $"The namespace-area fixture reported {string.Join(", ", Codes(mismatch))}.");

        var unresolved = Run(root, Path.Combine(fixtures, "invalid", "unresolved-trait-key"));
        var unresolvedOk = Codes(unresolved).SetEquals(["unresolved-trait-key"]);
        Record("UnresolvedKeyReported", unresolvedOk,
            unresolvedOk
                ? "The trait key that no constant declares is rejected with unresolved-trait-key."
                : $"The unresolved-key fixture reported {string.Join(", ", Codes(unresolved))}.");

        var tree = Run(root, Path.Combine(root, "src", "test"));
        var treeViolations = Violations(tree).Count;
        var treeScenarios = tree["counts"]!["scenarios"]!.GetValue<int>();
        var clean = treeViolations == 0 && treeScenarios > 0;
        Record("CoreTreeClean", clean,
            clean
                ? $"{treeScenarios} Core scenarios report no violation."
                : $"{treeScenarios} Core scenarios report {treeViolations} violation(s).");
    }

    /// <summary>Runs the gate over one root and reads its JSON evidence.</summary>
    private static JsonObject Run(string repositoryRoot, string target)
    {
        var gate = Path.Combine(repositoryRoot, "eng", "verify-test-layout.py");
        var run = global::LowlandTech.Core.Tests.Refinement.GateRunner.Run(
            gate, repositoryRoot, "--root", target, "--json");
        if (string.IsNullOrWhiteSpace(run.StandardOutput)) return new JsonObject();
        return JsonNode.Parse(run.StandardOutput)!.AsObject();
    }

    private static List<JsonObject> Violations(JsonObject evidence)
        => (evidence["violations"] as JsonArray ?? []).Select(node => node!.AsObject()).ToList();

    private static HashSet<string> Codes(JsonObject evidence)
        => Violations(evidence).Select(violation => violation["code"]!.GetValue<string>()).ToHashSet();

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "AliasResolved": AliasResolved = outcome; break;
            case "DuplicateRejected": DuplicateRejected = outcome; break;
            case "NamespaceRequired": NamespaceRequired = outcome; break;
            case "NamespaceIdentityChecked": NamespaceIdentityChecked = outcome; break;
            case "UnresolvedKeyReported": UnresolvedKeyReported = outcome; break;
            case "CoreTreeClean": CoreTreeClean = outcome; break;
        }
    }
}
