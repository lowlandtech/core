using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace LowlandTech.Core.Tests;

/// <summary>
/// The acceptance inventory of CORE0005-US01-SC01. Every Fact observes the real
/// contract: the wire values of the merge-request states, the transitions the
/// Core processor owns, the lifecycle of a feature proposal and the immutable
/// package that carries them for the clients.
/// </summary>
public sealed class GitFlowClientContractAcceptanceTests
{
    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "The Fact observes the wire format of the merge-request status, so it reads and writes the contract over the reflection-based JSON stack. The test assembly is not trimmed and the contract assembly it reads publishes a converter that needs no reflection.")]
    public void UAC01_MergeRequestStatesRetainLegacyMeaning()
    {
        // The wire values of the published members never change.
        ((int)MergeRequestStatus.Open).ShouldBe(0);
        ((int)MergeRequestStatus.Approved).ShouldBe(1);
        ((int)MergeRequestStatus.Rejected).ShouldBe(2);
        ((int)MergeRequestStatus.Reviewing).ShouldBe(3);
        ((int)MergeRequestStatus.Merged).ShouldBe(4);
        ((int)MergeRequestStatus.Abandoned).ShouldBe(5);

        // The six states the client renders are the published ones.
        Enum.GetNames<MergeRequestStatus>().ShouldBe(
            ["Open", "Approved", "Rejected", "Reviewing", "Merged", "Abandoned"]);

        // A document of the older producer keeps its original meaning.
        var legacy = """
            {
              "Id": "6f8f7c1e-6f45-4a09-9c1c-2f4f0b9d5a11",
              "BranchId": "1b1f0b4a-9f3a-4a6b-9a24-3a5e2c9c7f21",
              "TargetBranch": 0,
              "Status": 0,
              "ProposedTransitions": []
            }
            """;
        var request = JsonSerializer.Deserialize<GraphMergeRequest>(legacy);
        request.ShouldNotBeNull();
        request!.Status.ShouldBe(MergeRequestStatus.Open);

        JsonSerializer.Deserialize<MergeRequestStatus>("0").ShouldBe(MergeRequestStatus.Open);
        JsonSerializer.Deserialize<MergeRequestStatus>("1").ShouldBe(MergeRequestStatus.Approved);
        JsonSerializer.Deserialize<MergeRequestStatus>("2").ShouldBe(MergeRequestStatus.Rejected);

        // The new states read from their stable wire value and from their name.
        JsonSerializer.Deserialize<MergeRequestStatus>("3").ShouldBe(MergeRequestStatus.Reviewing);
        JsonSerializer.Deserialize<MergeRequestStatus>("4").ShouldBe(MergeRequestStatus.Merged);
        JsonSerializer.Deserialize<MergeRequestStatus>("5").ShouldBe(MergeRequestStatus.Abandoned);
        JsonSerializer.Deserialize<MergeRequestStatus>("\"Reviewing\"").ShouldBe(MergeRequestStatus.Reviewing);
        JsonSerializer.Deserialize<MergeRequestStatus>("\"abandoned\"").ShouldBe(MergeRequestStatus.Abandoned);

        // The written wire value of every state is its stable number.
        JsonSerializer.Serialize(MergeRequestStatus.Open).ShouldBe("0");
        JsonSerializer.Serialize(MergeRequestStatus.Approved).ShouldBe("1");
        JsonSerializer.Serialize(MergeRequestStatus.Rejected).ShouldBe("2");
        JsonSerializer.Serialize(MergeRequestStatus.Reviewing).ShouldBe("3");
        JsonSerializer.Serialize(MergeRequestStatus.Merged).ShouldBe("4");
        JsonSerializer.Serialize(MergeRequestStatus.Abandoned).ShouldBe("5");

        // A value that names no state is refused instead of being read as one.
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MergeRequestStatus>("6"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MergeRequestStatus>("\"Settled\""));
    }

    [Fact]
    public async Task UAC02_MergeRequestActionsRespectState()
    {
        var processor = new MergeRequestProcessor([]);
        var cancellationToken = TestContext.Current.CancellationToken;

        // The processor exposes which live requests may still be answered.
        foreach (var live in new[] { MergeRequestStatus.Open, MergeRequestStatus.Reviewing })
        {
            processor.CanApprove(Request(live)).ShouldBeTrue();
            processor.CanReject(Request(live)).ShouldBeTrue();
            processor.CanAbandon(Request(live)).ShouldBeTrue();
        }
        foreach (var settled in new[]
                 {
                     MergeRequestStatus.Approved, MergeRequestStatus.Rejected,
                     MergeRequestStatus.Merged, MergeRequestStatus.Abandoned,
                 })
        {
            processor.CanApprove(Request(settled)).ShouldBeFalse();
            processor.CanReject(Request(settled)).ShouldBeFalse();
            processor.CanAbandon(Request(settled)).ShouldBeFalse();
        }

        // A request reaches the reviewing state through the processor.
        var request = Request(MergeRequestStatus.Open);
        await processor.StartReviewAsync(request, cancellationToken);
        request.Status.ShouldBe(MergeRequestStatus.Reviewing);

        // The approval of a reviewing request replays and advances it.
        var approval = await processor.ApproveAsync(request, cancellationToken);
        request.Status.ShouldBe(MergeRequestStatus.Approved);
        approval.MergedEvent.MergeRequestId.ShouldBe(request.Id);

        // The approved request reaches the merged state through the processor.
        await processor.MergeAsync(request, cancellationToken);
        request.Status.ShouldBe(MergeRequestStatus.Merged);

        // A terminal request refuses every further action.
        await Should.ThrowAsync<InvalidOperationException>(() => processor.ApproveAsync(request, cancellationToken));
        await Should.ThrowAsync<InvalidOperationException>(() => processor.RejectAsync(request, cancellationToken));
        await Should.ThrowAsync<InvalidOperationException>(() => processor.AbandonAsync(request, cancellationToken));
        await Should.ThrowAsync<InvalidOperationException>(() => processor.MergeAsync(request, cancellationToken));

        // A rejected request is refused as well.
        var rejected = Request(MergeRequestStatus.Reviewing);
        await processor.RejectAsync(rejected, cancellationToken);
        rejected.Status.ShouldBe(MergeRequestStatus.Rejected);
        await Should.ThrowAsync<InvalidOperationException>(() => processor.ApproveAsync(rejected, cancellationToken));
        await Should.ThrowAsync<InvalidOperationException>(() => processor.StartReviewAsync(rejected, cancellationToken));

        // An abandoned request is refused as well.
        var abandoned = Request(MergeRequestStatus.Reviewing);
        await processor.AbandonAsync(abandoned, cancellationToken);
        abandoned.Status.ShouldBe(MergeRequestStatus.Abandoned);
        await Should.ThrowAsync<InvalidOperationException>(() => processor.ApproveAsync(abandoned, cancellationToken));
        await Should.ThrowAsync<InvalidOperationException>(() => processor.AbandonAsync(abandoned, cancellationToken));

        // A review is entered once.
        var review = Request(MergeRequestStatus.Open);
        await processor.StartReviewAsync(review, cancellationToken);
        await Should.ThrowAsync<InvalidOperationException>(() => processor.StartReviewAsync(review, cancellationToken));

        // An open request is approved directly as well.
        var open = Request(MergeRequestStatus.Open);
        await processor.ApproveAsync(open, cancellationToken);
        open.Status.ShouldBe(MergeRequestStatus.Approved);
    }

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "The Fact observes the wire format of the feature-proposal document, so it reads and writes the contract over the reflection-based JSON stack. The test assembly is not trimmed.")]
    public void UAC03_FeatureProposalExposesClientState()
    {
        // The client surface reads the title, the lifecycle and the specification.
        var proposal = Proposal();
        proposal.Title.ShouldBe("Notify the customer");
        proposal.Specification.ShouldNotBeNull();
        proposal.Specification!.ShouldContain("Scenario: notify a customer");
        proposal.Status.ShouldBe(FeatureProposalStatus.Draft);
        proposal.CanSubmit.ShouldBeTrue();
        proposal.CanDiscard.ShouldBeTrue();

        // Submit and discard belong to the draft.
        proposal.Submit();
        proposal.Status.ShouldBe(FeatureProposalStatus.Reviewed);
        proposal.CanSubmit.ShouldBeFalse();
        proposal.CanDiscard.ShouldBeFalse();
        Should.Throw<InvalidOperationException>(proposal.Submit);
        Should.Throw<InvalidOperationException>(proposal.Discard);

        // A reviewed proposal reaches the accepted and the rejected state.
        var accepted = Proposal();
        accepted.Submit();
        accepted.Accept();
        accepted.Status.ShouldBe(FeatureProposalStatus.Accepted);
        Should.Throw<InvalidOperationException>(accepted.Accept);

        var rejected = Proposal();
        rejected.Submit();
        rejected.Reject();
        rejected.Status.ShouldBe(FeatureProposalStatus.Rejected);

        // A draft is discarded.
        var discarded = Proposal();
        discarded.Discard();
        discarded.Status.ShouldBe(FeatureProposalStatus.Rejected);
        Should.Throw<InvalidOperationException>(discarded.Discard);

        // A legacy document that carries none of the new fields keeps its meaning.
        var legacy = """
            {
              "Id": "6f8f7c1e-6f45-4a09-9c1c-2f4f0b9d5a11",
              "BranchId": "1b1f0b4a-9f3a-4a6b-9a24-3a5e2c9c7f21",
              "ValidatedPath": ["graph.load"],
              "Proposals": [ { "EdgeKey": "notify.customer", "Description": "The missing edge." } ],
              "CreatedAt": "2026-10-27T10:00:00+00:00"
            }
            """;
        var document = JsonSerializer.Deserialize<FeatureProposalDocument>(legacy);
        document.ShouldNotBeNull();
        document!.Title.ShouldBeNull();
        document.Specification.ShouldBeNull();
        document.Status.ShouldBe(FeatureProposalStatus.Draft);
        document.CanSubmit.ShouldBeTrue();
        document.CanDiscard.ShouldBeTrue();

        // The lifecycle of the document survives a round trip over its wire value.
        var wire = JsonSerializer.Serialize(proposal);
        JsonSerializer.Deserialize<FeatureProposalDocument>(wire)!.Status
            .ShouldBe(FeatureProposalStatus.Reviewed);
    }

    [Fact]
    public void UAC04_PackageConsumerReadsGitFlowContract()
    {
        // The contract the isolated package consumer compiles against.
        var names = PublicTypeNames(typeof(MergeRequestStatus).Assembly);
        names.ShouldContain("MergeRequestStatus");
        names.ShouldContain("MergeRequestStatusLifecycle");
        names.ShouldContain("MergeRequestStatusWireConverter");
        names.ShouldContain("FeatureProposalStatus");
        names.ShouldContain("FeatureProposalDocument");

        // The immutable package that carries the same contract.
        var package = FindPackage();
        package.ShouldNotBeNull(
            "No immutable LowlandTech.Core package carries the git-flow contract. "
            + "Run eng/build-local.ps1 to stage and publish the candidate.");
        package!.Version.ShouldNotBeNullOrEmpty();
        package.Version.ShouldMatch(@"^2026\.10\.3-[1-9][0-9]*$");
        package.Names.ShouldContain("MergeRequestStatus");
        package.Names.ShouldContain("FeatureProposalStatus");
        package.Names.ShouldContain("FeatureProposalDocument");

        // The package keeps the exact closure the producer pins.
        package.Dependencies.ShouldContain("LowlandTech.Components.Core");
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
        var root = RepositoryRoot();
        if (root is not null)
        {
            var version = PackageVersionOf(typeof(MergeRequestStatus).Assembly);
            if (version is not null)
            {
                var staged = Path.Combine(root, "artifacts", "local-packages", version);
                if (Directory.Exists(staged))
                {
                    var candidate = Directory
                        .EnumerateFiles(staged, "LowlandTech.Core.*.nupkg")
                        .FirstOrDefault();
                    var package = candidate is null ? null : PublishedPackage.Read(candidate);
                    if (package is not null) return package;
                }
            }
        }

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

    /// <summary>The package version the Core assembly of this build carries.</summary>
    private static string? PackageVersionOf(Assembly assembly)
    {
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
        var plus = informational.IndexOf('+', StringComparison.Ordinal);
        var version = plus < 0 ? informational : informational[..plus];
        return version.Length == 0 ? null : version;
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
