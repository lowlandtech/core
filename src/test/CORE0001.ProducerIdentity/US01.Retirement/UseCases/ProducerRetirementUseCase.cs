using System.Text.Json.Nodes;

namespace LowlandTech.Core.Tests.CORE0001.ProducerIdentity.US01.Retirement.UseCases;

/// <summary>
/// The acceptance inventory of CORE0001-US02-SC01. Core owns the package
/// trimmability contract and its own producer proof, so this use case measures
/// the producer uniqueness that UAC01 asks for: the canonical Core checkout is
/// the only place that publishes a Core package identity.
///
/// The measurement is the repository owned gate at eng/verify-producers.py. It
/// reads the effective MSBuild properties of every project that can declare a
/// Core package identity, so a checkout that publishes a package while its own
/// project file says it cannot is still found.
///
/// The gate reports a competing producer outside the Core repository with exit
/// code 2. That is a finding for the lane that owns that checkout, not a Core
/// owned failure, so the observations here require the Core owned invariants
/// and record the external producers with their owners.
/// </summary>
public sealed class ProducerRetirementUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    /// <summary>The canonical producer of each Core package identity.</summary>
    private static readonly (string PackageId, string RelativePath)[] CanonicalProducers =
    {
        ("LowlandTech.Core", "src/lib/LowlandTech.Core.csproj"),
        ("LowlandTech.Core.Contracts", "src/contracts/LowlandTech.Core.Contracts.csproj")
    };

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The canonical producer of every Core identity publishes it.</summary>
    public bool? CanonicalProducersPublish { get; private set; }

    /// <summary>The gate reports no violation that the Core lane owns.</summary>
    public bool? NoCoreOwnedViolation { get; private set; }

    /// <summary>Every producer outside the Core lane is named with its owner.</summary>
    public bool? CompetingProducersHaveOwners { get; private set; }

    /// <summary>Every retired producer refuses to publish.</summary>
    public bool? RetiredProducersRefuse { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        var repositoryRoot = global::LowlandTech.Core.Tests.Refinement.GateRunner.RepositoryRoot("verify-producers.py");
        if (repositoryRoot is null)
        {
            Record("CanonicalProducersPublish", false, "The Core repository root was not found.");
            Record("NoCoreOwnedViolation", false, "The Core repository root was not found.");
            Record("CompetingProducersHaveOwners", false, "The Core repository root was not found.");
            Record("RetiredProducersRefuse", false, "The Core repository root was not found.");
            return Task.CompletedTask;
        }

        var gate = Path.Combine(repositoryRoot, "eng", "verify-producers.py");
        if (!File.Exists(gate))
        {
            Record("CanonicalProducersPublish", false, $"The gate '{gate}' is missing.");
            Record("NoCoreOwnedViolation", false, $"The gate '{gate}' is missing.");
            Record("CompetingProducersHaveOwners", false, $"The gate '{gate}' is missing.");
            Record("RetiredProducersRefuse", false, $"The gate '{gate}' is missing.");
            return Task.CompletedTask;
        }

        var workspaceRoot = Directory.GetParent(repositoryRoot)!.FullName;
        var run = global::LowlandTech.Core.Tests.Refinement.GateRunner.Run(
            gate, repositoryRoot, "--repository-root", repositoryRoot, "--workspace-root", workspaceRoot, "--json");

        // Exit code 0 means the canonical producer is the only producer and exit
        // code 2 means the Core owned invariants hold while another checkout can
        // still publish. Exit code 1 is a Core owned violation and exit code 3
        // means the measurement did not run at all.
        var acceptable = run.ExitCode is 0 or 2;
        JsonObject evidence;
        try
        {
            evidence = JsonNode.Parse(run.StandardOutput)!.AsObject();
        }
        catch (Exception exception)
        {
            Record("CanonicalProducersPublish", false, $"The gate did not report its evidence: {exception.Message}");
            Record("NoCoreOwnedViolation", false, $"The gate did not report its evidence: {exception.Message}");
            Record("CompetingProducersHaveOwners", false, $"The gate did not report its evidence: {exception.Message}");
            Record("RetiredProducersRefuse", false, $"The gate did not report its evidence: {exception.Message}");
            return Task.CompletedTask;
        }

        var canonical = evidence["canonical"]!.AsArray();
        var canonicalOk = canonical.Count == CanonicalProducers.Length
                          && CanonicalProducers.All(expected =>
                          {
                              var producer = canonical.SingleOrDefault(node =>
                                  node!["packageId"]!.GetValue<string>() == expected.PackageId);
                              if (producer is null) return false;
                              if (!producer["packable"]!.GetValue<bool>()) return false;
                              if (producer["owner"]!.GetValue<string>() != "core") return false;
                              var relative = Path.GetRelativePath(repositoryRoot,
                                  producer["project"]!.GetValue<string>()).Replace('\\', '/');
                              return relative == expected.RelativePath;
                          });
        Record("CanonicalProducersPublish", canonicalOk,
            $"{canonical.Count} canonical producers were reported and {CanonicalProducers.Length} are expected.");

        var coreOwned = acceptable
                       && evidence["coreViolations"]!.AsArray().Count == 0
                       && evidence["missingCanonicalProducers"]!.AsArray().Count == 0
                       && evidence["unevaluated"]!.AsArray().Count == 0;
        Record("NoCoreOwnedViolation", coreOwned,
            $"The gate reported exit {run.ExitCode} with {evidence["coreViolations"]!.AsArray().Count} Core owned "
            + $"violations, {evidence["missingCanonicalProducers"]!.AsArray().Count} missing canonical producers "
            + $"and {evidence["unevaluated"]!.AsArray().Count} unevaluated projects.");

        var competing = evidence["competingProducers"]!.AsArray();
        var ownersOk = competing.All(node =>
        {
            var project = node!["project"]!.GetValue<string>();
            var relative = Path.GetRelativePath(repositoryRoot, project);
            var outside = relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative);
            var owner = node["owner"]!.GetValue<string>();
            return outside
                   && owner != "core"
                   && owner != "unknown"
                   && node["packable"]!.GetValue<bool>()
                   && !node["refusesForcedPack"]!.GetValue<bool>();
        });
        Record("CompetingProducersHaveOwners", ownersOk,
            $"{competing.Count} competing producers were reported outside the Core lane and every one names its owner.");

        var retired = evidence["retired"]!.AsArray();
        var retiredOk = retired.All(node =>
        {
            var packable = node!["packable"]!.GetValue<bool>();
            var refuses = node["refusesForcedPack"]!.GetValue<bool>();
            return !packable || refuses;
        });
        Record("RetiredProducersRefuse", retiredOk,
            $"{retired.Count} retired producers were reported and every one of them refuses to publish.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "CanonicalProducersPublish": CanonicalProducersPublish = outcome; break;
            case "NoCoreOwnedViolation": NoCoreOwnedViolation = outcome; break;
            case "CompetingProducersHaveOwners": CompetingProducersHaveOwners = outcome; break;
            case "RetiredProducersRefuse": RetiredProducersRefuse = outcome; break;
        }
    }
}
