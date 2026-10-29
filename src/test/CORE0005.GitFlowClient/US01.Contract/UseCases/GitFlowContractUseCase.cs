using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace LowlandTech.Core.Tests.CORE0005.GitFlowClient.US01.Contract.UseCases;

/// <summary>
/// The acceptance inventory of CORE0005-US01-SC01. Every observation is of the
/// real contract: the wire values of the merge-request states, the transitions
/// the Core processor owns, the lifecycle of a feature proposal and the
/// immutable package that carries them for the clients.
///
/// The observations are captured here so that every Fact of the scenario
/// asserts one of them against the production contract.
/// </summary>
public sealed class GitFlowContractUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The merge-request states keep their published wire meaning.</summary>
    public bool? MergeRequestStatesRetainLegacyMeaning { get; private set; }

    /// <summary>The merge-request actions respect the state of the request.</summary>
    public bool? MergeRequestActionsRespectState { get; private set; }

    /// <summary>The feature proposal exposes the state of the client surface.</summary>
    public bool? FeatureProposalExposesClientState { get; private set; }

    /// <summary>The immutable package carries the git-flow contract.</summary>
    public bool? PackageConsumerReadsGitFlowContract { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "The use case observes the wire format of the merge-request status and of the feature-proposal document, so it reads and writes the contract over the reflection-based JSON stack. The test assembly is not trimmed.")]
    public async Task InvokeAsync()
    {
        // The wire values of the published members never change.
        var numbers = new (MergeRequestStatus Status, int Value)[]
        {
            (MergeRequestStatus.Open, 0),
            (MergeRequestStatus.Approved, 1),
            (MergeRequestStatus.Rejected, 2),
            (MergeRequestStatus.Reviewing, 3),
            (MergeRequestStatus.Merged, 4),
            (MergeRequestStatus.Abandoned, 5)
        };
        var legacy = """
            {
              "Id": "6f8f7c1e-6f45-4a09-9c1c-2f4f0b9d5a11",
              "BranchId": "1b1f0b4a-9f3a-4a6b-9a24-3a5e2c9c7f21",
              "TargetBranch": 0,
              "Status": 0,
              "ProposedTransitions": []
            }
            """;
        var legacyRequest = JsonSerializer.Deserialize<GraphMergeRequest>(legacy);
        var unknownRefused = false;
        try
        {
            JsonSerializer.Deserialize<MergeRequestStatus>("6");
        }
        catch (JsonException)
        {
            unknownRefused = true;
        }
        var unknownNameRefused = false;
        try
        {
            JsonSerializer.Deserialize<MergeRequestStatus>("\"Settled\"");
        }
        catch (JsonException)
        {
            unknownNameRefused = true;
        }
        var statesKeepMeaning = numbers.All(pair => (int)pair.Status == pair.Value)
                                  && Enum.GetNames<MergeRequestStatus>().SequenceEqual(
                                      ["Open", "Approved", "Rejected", "Reviewing", "Merged", "Abandoned"])
                                  && legacyRequest is not null
                                  && legacyRequest.Status == MergeRequestStatus.Open
                                  && numbers.All(pair =>
                                      JsonSerializer.Deserialize<MergeRequestStatus>(pair.Value.ToString()) == pair.Status
                                      && JsonSerializer.Serialize(pair.Status) == pair.Value.ToString())
                                  && JsonSerializer.Deserialize<MergeRequestStatus>("\"Reviewing\"") == MergeRequestStatus.Reviewing
                                  && JsonSerializer.Deserialize<MergeRequestStatus>("\"abandoned\"") == MergeRequestStatus.Abandoned
                                  && unknownRefused
                                  && unknownNameRefused;
        Record("MergeRequestStatesRetainLegacyMeaning", statesKeepMeaning,
            $"The {numbers.Length} published states keep their wire values and their names.");

        // The processor owns the transitions of a request.
        var processor = new MergeRequestProcessor([]);
        var live = new[] { MergeRequestStatus.Open, MergeRequestStatus.Reviewing };
        var settled = new[]
        {
            MergeRequestStatus.Approved, MergeRequestStatus.Rejected,
            MergeRequestStatus.Merged, MergeRequestStatus.Abandoned
        };
        var permissions = live.All(status => processor.CanApprove(Request(status))
                                              && processor.CanReject(Request(status))
                                              && processor.CanAbandon(Request(status)))
                          && settled.All(status => !processor.CanApprove(Request(status))
                                                   && !processor.CanReject(Request(status))
                                                   && !processor.CanAbandon(Request(status)));
        var review = Request(MergeRequestStatus.Open);
        await processor.StartReviewAsync(review, CancellationToken.None);
        var approval = await processor.ApproveAsync(review, CancellationToken.None);
        await processor.MergeAsync(review, CancellationToken.None);
        var terminalRefused = await RefusesAsync(() => processor.ApproveAsync(review, CancellationToken.None))
                              && await RefusesAsync(() => processor.RejectAsync(review, CancellationToken.None))
                              && await RefusesAsync(() => processor.AbandonAsync(review, CancellationToken.None))
                              && await RefusesAsync(() => processor.MergeAsync(review, CancellationToken.None));
        var rejected = Request(MergeRequestStatus.Reviewing);
        await processor.RejectAsync(rejected, CancellationToken.None);
        var rejectedRefused = await RefusesAsync(() => processor.ApproveAsync(rejected, CancellationToken.None))
                              && await RefusesAsync(() => processor.StartReviewAsync(rejected, CancellationToken.None));
        var abandoned = Request(MergeRequestStatus.Reviewing);
        await processor.AbandonAsync(abandoned, CancellationToken.None);
        var abandonedRefused = await RefusesAsync(() => processor.ApproveAsync(abandoned, CancellationToken.None))
                               && await RefusesAsync(() => processor.AbandonAsync(abandoned, CancellationToken.None));
        var twice = Request(MergeRequestStatus.Open);
        await processor.StartReviewAsync(twice, CancellationToken.None);
        var reviewOnce = await RefusesAsync(() => processor.StartReviewAsync(twice, CancellationToken.None));
        var open = Request(MergeRequestStatus.Open);
        await processor.ApproveAsync(open, CancellationToken.None);
        var actionsRespectState = permissions
                                  && approval.MergedEvent.MergeRequestId == review.Id
                                  && review.Status == MergeRequestStatus.Merged
                              && terminalRefused
                              && rejected.Status == MergeRequestStatus.Rejected
                              && rejectedRefused
                              && abandoned.Status == MergeRequestStatus.Abandoned
                              && abandonedRefused
                              && reviewOnce
                              && open.Status == MergeRequestStatus.Approved;
        Record("MergeRequestActionsRespectState", actionsRespectState,
            "The processor moved the request through the reviewing, approved and merged states and refused every action that a settled state does not allow.");

        // The client surface of a feature proposal and its lifecycle.
        var proposal = Proposal();
        var draftState = proposal.Status == FeatureProposalStatus.Draft
                         && proposal.CanSubmit
                         && proposal.CanDiscard
                         && proposal.Title == "Notify the customer"
                         && proposal.Specification is not null
                         && proposal.Specification.Contains("Scenario: notify a customer", StringComparison.Ordinal);
        proposal.Submit();
        var afterSubmit = proposal.Status == FeatureProposalStatus.Reviewed
                          && !proposal.CanSubmit
                          && !proposal.CanDiscard
                          && Refuses(() => proposal.Submit())
                          && Refuses(() => proposal.Discard());
        var accepted = Proposal();
        accepted.Submit();
        accepted.Accept();
        var acceptedRefused = Refuses(() => accepted.Accept());
        var rejectedProposal = Proposal();
        rejectedProposal.Submit();
        rejectedProposal.Reject();
        var discarded = Proposal();
        discarded.Discard();
        var discardRefused = Refuses(() => discarded.Discard());
        var legacyProposal = """
            {
              "Id": "6f8f7c1e-6f45-4a09-9c1c-2f4f0b9d5a11",
              "BranchId": "1b1f0b4a-9f3a-4a6b-9a24-3a5e2c9c7f21",
              "ValidatedPath": ["graph.load"],
              "Proposals": [ { "EdgeKey": "notify.customer", "Description": "The missing edge." } ],
              "CreatedAt": "2026-10-27T10:00:00+00:00"
            }
            """;
        var legacyDocument = JsonSerializer.Deserialize<FeatureProposalDocument>(legacyProposal);
        var wire = JsonSerializer.Serialize(proposal);
        var proposalState = draftState
                            && afterSubmit
                            && accepted.Status == FeatureProposalStatus.Accepted
                            && acceptedRefused
                            && rejectedProposal.Status == FeatureProposalStatus.Rejected
                            && discarded.Status == FeatureProposalStatus.Rejected
                            && discardRefused
                            && legacyDocument is not null
                            && legacyDocument.Title is null
                            && legacyDocument.Specification is null
                            && legacyDocument.Status == FeatureProposalStatus.Draft
                            && legacyDocument.CanSubmit
                            && legacyDocument.CanDiscard
                            && JsonSerializer.Deserialize<FeatureProposalDocument>(wire)!.Status == FeatureProposalStatus.Reviewed;
        Record("FeatureProposalExposesClientState", proposalState,
            $"The proposal moved from the draft to the {proposal.Status} state and a legacy document kept its meaning.");

        // The immutable package that carries the same contract.
        var names = PublicTypeNames(typeof(MergeRequestStatus).Assembly);
        var expected = new[]
        {
            "MergeRequestStatus", "MergeRequestStatusLifecycle", "MergeRequestStatusWireConverter",
            "FeatureProposalStatus", "FeatureProposalDocument"
        };
        var package = FindPackage();
        var packageOk = expected.All(names.Contains)
                        && package is not null
                        && !string.IsNullOrEmpty(package.Version)
                        && System.Text.RegularExpressions.Regex.IsMatch(package.Version!, @"^2026\.10\.3-[1-9][0-9]*$")
                        && package.Names.Contains("MergeRequestStatus")
                        && package.Names.Contains("FeatureProposalStatus")
                        && package.Names.Contains("FeatureProposalDocument")
                        && package.Dependencies.Contains("LowlandTech.Components.Core");
        Record("PackageConsumerReadsGitFlowContract", packageOk,
            package is null
                ? "No immutable LowlandTech.Core package carries the git-flow contract."
                : $"The package {package.Version} carries the git-flow contract.");

        await Task.CompletedTask;
    }

    private static bool Refuses(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private static async Task<bool> RefusesAsync(Func<Task> action)
    {
        try
        {
            await action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "MergeRequestStatesRetainLegacyMeaning": MergeRequestStatesRetainLegacyMeaning = outcome; break;
            case "MergeRequestActionsRespectState": MergeRequestActionsRespectState = outcome; break;
            case "FeatureProposalExposesClientState": FeatureProposalExposesClientState = outcome; break;
            case "PackageConsumerReadsGitFlowContract": PackageConsumerReadsGitFlowContract = outcome; break;
        }
    }

    /// <summary>A merge request in the given state.</summary>
    private static GraphMergeRequest Request(MergeRequestStatus status) => new()
    {
        Id = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        TargetBranch = BranchType.Feature,
        Status = status,
        ProposedTransitions = [],
    };

    /// <summary>A feature proposal in the draft state.</summary>
    private static FeatureProposalDocument Proposal() => new()
    {
        Id = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        ValidatedPath = ["graph.load"],
        Proposals = [new FeatureProposal { EdgeKey = "notify.customer", Description = "The missing edge." }],
        Title = "Notify the customer",
        Specification = """
            Feature: customer notification
              Scenario: notify a customer
                Given a customer
                When the customer is notified
                Then the notification is sent
            """,
    };

    /// <summary>
    /// The immutable Core package that carries the git-flow contract: the staged
    /// candidate of this release when it exists, and the published package of the
    /// local feed otherwise.
    /// </summary>
    private static PublishedPackage? FindPackage()
    {
        var feed = LocalFeed();
        if (feed is null) return null;
        foreach (var path in Directory.EnumerateFiles(feed, "LowlandTech.Core.*.nupkg"))
        {
            var package = PublishedPackage.Read(path);
            if (package is not null && package.Names.Contains("FeatureProposalStatus")) return package;
        }
        return null;
    }

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

    /// <summary>The public type names of an assembly.</summary>
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

    /// <summary>A published Core package of the local feed.</summary>
    private sealed record PublishedPackage(
        string FilePath,
        string? Version,
        IReadOnlyList<string> Names,
        IReadOnlyList<string> Dependencies)
    {
        /// <summary>Reads a published package.</summary>
        public static PublishedPackage? Read(string path)
        {
            using var archive = ZipFile.OpenRead(path);
            var assembly = archive.Entries.FirstOrDefault(entry => entry.FullName.StartsWith("lib/", StringComparison.Ordinal)
                && entry.FullName.EndsWith(".dll", StringComparison.Ordinal));
            var nuspec = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
            if (assembly is null || nuspec is null) return null;

            var extracted = Path.Combine(Path.GetTempPath(), "core-gitflow-probe-" + Guid.NewGuid().ToString("N") + ".dll");
            try
            {
                assembly.ExtractToFile(extracted, overwrite: true);
                var names = new List<string>();
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
                }

                string? version = null;
                var dependencies = new List<string>();
                using (var reader = new StreamReader(nuspec.Open()))
                {
                    var document = System.Xml.Linq.XDocument.Parse(reader.ReadToEnd());
                    // The nuspec carries a default namespace, so every element is
                    // read by its local name.
                    version = document.Descendants()
                        .FirstOrDefault(element => element.Name.LocalName == "version")?.Value;
                    dependencies.AddRange(document.Descendants()
                        .Where(element => element.Name.LocalName == "dependency")
                        .Select(element => (string?)element.Attribute("id") ?? string.Empty)
                        .Where(id => id.Length > 0));
                }
                return new(path, version, names, dependencies);
            }
            finally
            {
                if (File.Exists(extracted)) File.Delete(extracted);
            }
        }
    }
}
