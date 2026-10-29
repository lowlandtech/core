using System.Text.Json.Nodes;

namespace LowlandTech.Core.Tests.CORE0001.DependencyClosure.US01.Contract.UseCases;

/// <summary>
/// The Core owned half of UAC02 of CORE0001-US02-SC01: the canonical producer
/// has to pin the generation of the Components.Core package that carries the
/// startup contract, and that pin has to stay unambiguous.
///
/// The measurement is the repository owned gate at eng/verify-closure.py. The
/// gate reports the version that a direct reference resolves to, the versions
/// that shadow the pin and the reference that the newest published batch of
/// the Components lane declares. A version that shadows the pin is a finding
/// for the lane that owns the dependency, not a Core owned failure, so the
/// observations here require the Core owned invariants and record every
/// divergence with its owner.
/// </summary>
public sealed class DependencyClosureUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    /// <summary>
    /// The types of the dependency that the trimmed consumer of the producer
    /// builds against, so a generation without them cannot satisfy the
    /// producer's own contract.
    /// </summary>
    private static readonly string[] StartupContractTypes =
    {
        "LowlandTech.Components.Core.ComponentsStartupMap",
        "LowlandTech.Components.Core.ComponentsCoreStartup",
        "LowlandTech.Components.Core.ComponentsStartupBinding"
    };

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The producer declares an exact dependency that the feed carries.</summary>
    public bool? ProducerDeclaresExactPin { get; private set; }

    /// <summary>The pinned generation is the one that every resolver selects.</summary>
    public bool? PinIsUnambiguous { get; private set; }

    /// <summary>The pinned generation carries the startup contract.</summary>
    public bool? PinnedGenerationCarriesTheStartupContract { get; private set; }

    /// <summary>The published candidate declares a dependency that the feed carries.</summary>
    public bool? PublishedCandidateDeclaresThePin { get; private set; }

    /// <summary>Every divergence is named with the lane that owns it.</summary>
    public bool? DivergencesHaveOwners { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        var repositoryRoot = global::LowlandTech.Core.Tests.Refinement.GateRunner.RepositoryRoot("verify-closure.py");
        if (repositoryRoot is null)
        {
            Record("ProducerDeclaresExactPin", false, "The Core repository root was not found.");
            Record("PinIsUnambiguous", false, "The Core repository root was not found.");
            Record("PinnedGenerationCarriesTheStartupContract", false, "The Core repository root was not found.");
            Record("PublishedCandidateDeclaresThePin", false, "The Core repository root was not found.");
            Record("DivergencesHaveOwners", false, "The Core repository root was not found.");
            return Task.CompletedTask;
        }

        var gate = Path.Combine(repositoryRoot, "eng", "verify-closure.py");
        var workspaceRoot = Directory.GetParent(repositoryRoot)!.FullName;
        var feedRoot = Path.Combine(workspaceRoot, "packages");
        var run = global::LowlandTech.Core.Tests.Refinement.GateRunner.Run(
            gate, repositoryRoot, "--repository-root", repositoryRoot, "--workspace-root", workspaceRoot, "--json");

        // Exit code 0 means the pinned generation is unambiguous and exit code 2
        // means the Core owned invariants hold while another generation shadows
        // the pin. Exit code 1 is a Core owned violation and exit code 3 means
        // the measurement did not run at all.
        if (run.ExitCode is not (0 or 2))
        {
            var failure = $"The closure gate reported exit code {run.ExitCode}. {run.StandardError}{run.StandardOutput}";
            Record("ProducerDeclaresExactPin", false, failure);
            Record("PinIsUnambiguous", false, failure);
            Record("PinnedGenerationCarriesTheStartupContract", false, failure);
            Record("PublishedCandidateDeclaresThePin", false, failure);
            Record("DivergencesHaveOwners", false, failure);
            return Task.CompletedTask;
        }

        var evidence = JsonNode.Parse(run.StandardOutput)!.AsObject();
        var pinned = evidence["pinned"]!.GetValue<string>();
        var available = evidence["availableVersions"]!.AsArray()
            .Select(node => node!.GetValue<string>()).ToList();

        var exact = evidence["pinnedIsExact"]!.GetValue<bool>()
                    && evidence["pinnedInFeed"]!.GetValue<bool>()
                    && evidence["coreViolations"]!.AsArray().Count == 0;
        Record("ProducerDeclaresExactPin", exact,
            $"The producer pins {pinned}, the pin is exact and in the feed, and the gate reports "
            + $"{evidence["coreViolations"]!.AsArray().Count} Core owned violations.");

        var unambiguous = evidence["directResolution"]!.GetValue<string>() == pinned
                          && evidence["highestAvailable"]!.GetValue<string>() == pinned
                          && evidence["shadowingVersions"]!.AsArray().Count == 0;
        Record("PinIsUnambiguous", unambiguous,
            $"The direct resolution is {evidence["directResolution"]!.GetValue<string>()}, the highest available "
            + $"version is {evidence["highestAvailable"]!.GetValue<string>()} and "
            + $"{evidence["shadowingVersions"]!.AsArray().Count} versions shadow the pin.");

        var pinnedPackage = Path.Combine(feedRoot, $"LowlandTech.Components.Core.{pinned}.nupkg");
        var carries = File.Exists(pinnedPackage);
        if (carries)
        {
            var types = global::LowlandTech.Core.Tests.Refinement.PackageFeed.PublicTypes(pinnedPackage);
            carries = StartupContractTypes.All(types.Contains);
        }
        Record("PinnedGenerationCarriesTheStartupContract", carries,
            carries
                ? $"The pinned generation {pinned} carries {StartupContractTypes.Length} startup contract types."
                : $"The pinned generation {pinned} does not carry the startup contract.");

        var candidate = global::LowlandTech.Core.Tests.Refinement.PackageFeed.NewestCoreCandidate(feedRoot);
        var declared = candidate is null
            ? null
            : global::LowlandTech.Core.Tests.Refinement.PackageFeed.DeclaredDependency(
                candidate, "LowlandTech.Components.Core");
        var declaredGeneration = declared?.Trim('[', ']');
        var consistent = candidate is not null
                         && declared is not null
                         && declared.StartsWith('[')
                         && declared.EndsWith(']')
                         && available.Contains(declaredGeneration)
                         && global::LowlandTech.Core.Tests.Refinement.PackageFeed.ComparePublication(
                                declaredGeneration!, pinned) <= 0;
        Record("PublishedCandidateDeclaresThePin", consistent,
            consistent
                ? $"The published candidate declares {declared} and the feed carries that generation."
                : $"The published candidate declares {declared} and the source pins {pinned}.");

        var hazards = evidence["externalHazards"]!.AsArray();
        var owned = hazards.All(node =>
        {
            var owner = node!["owner"]!.GetValue<string>();
            return owner == evidence["dependencyOwner"]!.GetValue<string>() && owner != "core";
        });
        Record("DivergencesHaveOwners", owned,
            $"{hazards.Count} divergences were reported and every one names the lane that owns the dependency.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "ProducerDeclaresExactPin": ProducerDeclaresExactPin = outcome; break;
            case "PinIsUnambiguous": PinIsUnambiguous = outcome; break;
            case "PinnedGenerationCarriesTheStartupContract": PinnedGenerationCarriesTheStartupContract = outcome; break;
            case "PublishedCandidateDeclaresThePin": PublishedCandidateDeclaresThePin = outcome; break;
            case "DivergencesHaveOwners": DivergencesHaveOwners = outcome; break;
        }
    }
}
