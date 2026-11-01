using System.Text.Json;
using LowlandTech.Core.Contracts;
using LowlandTech.Core.Contracts.Sessions;

namespace LowlandTech.Core.Tests.CORE0009.SessionContent.US01.Contract.UseCases;

/// <summary>
/// The acceptance inventory of CORE0009-US01-SC01. The contract is observed
/// through the assembly that the consumers reference and through the feed that
/// carries the published generations, so that the observations are of the
/// shipped contract and not of a shape invented by this lane.
/// </summary>
public sealed class SessionContentContractUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    /// <summary>The contract package that carried the correction before this contract.</summary>
    private const string PublishedContractPackage = "LowlandTech.Core.Contracts.2026.10.4-20261031113000.nupkg";

    /// <summary>The digest of that published package.</summary>
    private const string PublishedContractSha256 = "3b8f9ce17631f904701591d150d6bcddf39069c06f59e3644bf1407112543334";

    /// <summary>The portable names that this contract owns.</summary>
    private static readonly string[] PortableSessionTypes =
    [
        "LowlandTech.Core.Contracts.Sessions.ISessionContentProvider",
        "LowlandTech.Core.Contracts.Sessions.SessionContentDescriptor",
        "LowlandTech.Core.Contracts.Sessions.SessionContentIntent",
        "LowlandTech.Core.Contracts.Sessions.SessionContentTargets"
    ];

    /// <summary>The host behaviour that this contract must not carry.</summary>
    private static readonly string[] HostBehaviourTypes =
    [
        "LowlandTech.Core.Contracts.Sessions.SessionContentAdmission",
        "LowlandTech.Core.Contracts.Sessions.SessionContentBoundary",
        "LowlandTech.Core.Contracts.Sessions.SessionContentContext",
        "LowlandTech.Core.Contracts.Sessions.SessionContentLayoutInventory",
        "LowlandTech.Core.Contracts.Sessions.SessionContentMount",
        "LowlandTech.Core.Contracts.Sessions.SessionContentPublisher"
    ];

    /// <summary>The portable plugin declaration names that this contract must not change.</summary>
    private static readonly string[] PluginDeclarationTypes =
    [
        "LowlandTech.Core.Contracts.Plugins.PortablePluginContentHash",
        "LowlandTech.Core.Contracts.Plugins.PortablePluginIdentity",
        "LowlandTech.Core.Contracts.Plugins.PortablePluginManifestDeclaration",
        "LowlandTech.Core.Contracts.Plugins.PortablePluginRevision",
        "LowlandTech.Core.Contracts.Plugins.PortablePluginScope"
    ];

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The descriptor is data only and round trips through the contract's own context.</summary>
    public bool? TheDescriptorIsDataOnly { get; private set; }

    /// <summary>The contract carries the portable surface and no host behaviour.</summary>
    public bool? TheContractCarriesOnlyThePortableSurface { get; private set; }

    /// <summary>A host reads the descriptor of a session through the provider.</summary>
    public bool? AHostReadsTheDescriptorThroughTheProvider { get; private set; }

    /// <summary>The portable plugin declaration surface of the package is unchanged.</summary>
    public bool? ThePluginDeclarationSurfaceIsUnchanged { get; private set; }

    /// <summary>The published contract versions stay available and unchanged.</summary>
    public bool? ThePublishedVersionsStayAvailable { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, "LowlandTech.Core.Contracts.dll");
        if (!File.Exists(assemblyPath))
        {
            Record("TheDescriptorIsDataOnly", false, "The contract assembly was not found next to the acceptance.");
            Record("TheContractCarriesOnlyThePortableSurface", false, "The contract assembly was not found next to the acceptance.");
            Record("AHostReadsTheDescriptorThroughTheProvider", false, "The contract assembly was not found next to the acceptance.");
            Record("ThePluginDeclarationSurfaceIsUnchanged", false, "The contract assembly was not found next to the acceptance.");
            Record("ThePublishedVersionsStayAvailable", false, "The contract assembly was not found next to the acceptance.");
            return Task.CompletedTask;
        }

        var exported = global::LowlandTech.Core.Tests.Refinement.PackageFeed.PublicTypesOfAssembly(assemblyPath);

        // The descriptor is data only: it round trips through the source
        // generated context of the contract, and it carries no host type.
        var sessionId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var descriptor = new SessionContentDescriptor
        {
            Version = SessionContentDescriptor.SupportedVersion,
            SessionId = sessionId,
            ContextId = null,
            Renderer = "session-content",
            RendererVersion = "1.0.0",
            Assets = ["asset-a", "asset-b"],
            Target = SessionContentTargets.SessionContent,
            Layout = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Cols"] = "12" },
            Intents = [new SessionContentIntent("navigation", SessionContentTargets.ExplicitHostSlot)],
            Transitions = ["harness.list-main-intents"]
        };
        var json = JsonSerializer.Serialize(descriptor, TransitionContractJsonContext.Default.SessionContentDescriptor);
        var roundTrip = JsonSerializer.Deserialize(json, TransitionContractJsonContext.Default.SessionContentDescriptor);
        var dataOnly = roundTrip is not null
                       && roundTrip.Version == descriptor.Version
                       && roundTrip.SessionId == descriptor.SessionId
                       && roundTrip.Renderer == descriptor.Renderer
                       && roundTrip.RendererVersion == descriptor.RendererVersion
                       && roundTrip.Target == descriptor.Target
                       && roundTrip.Assets.SequenceEqual(descriptor.Assets)
                       && roundTrip.Layout["Cols"] == "12"
                       && roundTrip.Intents.Count == 1
                       && roundTrip.Intents[0].Kind == "navigation"
                       && roundTrip.Transitions.SequenceEqual(descriptor.Transitions);
        Record("TheDescriptorIsDataOnly", dataOnly,
            dataOnly
                ? $"The descriptor round trips through the contract's own context as {json.Length} bytes of data."
                : $"The descriptor did not round trip: {json}");

        // The contract owns the portable surface only. The publisher, the
        // admission and the mount stay with the lanes that own them.
        var carriesPortable = PortableSessionTypes.All(exported.Contains);
        var carriesHostBehaviour = HostBehaviourTypes.Any(exported.Contains);
        var onlyPortable = carriesPortable && !carriesHostBehaviour;
        Record("TheContractCarriesOnlyThePortableSurface", onlyPortable,
            onlyPortable
                ? $"The contract publishes {PortableSessionTypes.Length} portable session names and no host behaviour."
                : $"The portable names present: {carriesPortable}; the host behaviour present: {carriesHostBehaviour}.");

        // A host reads the descriptor of a session through the provider.
        var provider = new StubProvider(descriptor);
        var read = provider.GetAsync(sessionId).GetAwaiter().GetResult();
        var hostRead = read is not null
                       && read.SessionId == sessionId
                       && read.Renderer == "session-content"
                       && read.RendererVersion == "1.0.0"
                       && read.Assets.Count == 2
                       && read.Target == SessionContentTargets.SessionContent
                       && read.Layout.Count == 1
                       && read.Intents.Count == 1
                       && read.Transitions.Count == 1;
        Record("AHostReadsTheDescriptorThroughTheProvider", hostRead,
            hostRead
                ? "The host read the descriptor of the session through the provider of the contract."
                : $"The host read no descriptor: {read?.ToString() ?? "none"}");

        // The portable plugin declaration surface is not changed by this contract.
        var pluginSurface = PluginDeclarationTypes.All(exported.Contains);
        Record("ThePluginDeclarationSurfaceIsUnchanged", pluginSurface,
            pluginSurface
                ? $"The contract still publishes {PluginDeclarationTypes.Length} portable plugin declaration names."
                : "The contract no longer publishes every portable plugin declaration name.");

        // The published generations stay available and unchanged.
        var repositoryRoot = global::LowlandTech.Core.Tests.Refinement.GateRunner.RepositoryRoot("verify-closure.py");
        var feedRoot = repositoryRoot is null
            ? null
            : Path.Combine(Directory.GetParent(repositoryRoot)!.FullName, "packages");
        var published = feedRoot is null ? null : Path.Combine(feedRoot, PublishedContractPackage);
        var available = published is not null
                        && File.Exists(published)
                        && global::LowlandTech.Core.Tests.Refinement.PackageFeed.Sha256(published) == PublishedContractSha256;
        Record("ThePublishedVersionsStayAvailable", available,
            available
                ? $"The published contract {PublishedContractPackage} is still in the feed with its recorded digest."
                : $"The published contract {PublishedContractPackage} is not in the feed with its recorded digest.");

        return Task.CompletedTask;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "TheDescriptorIsDataOnly":
                TheDescriptorIsDataOnly = outcome;
                break;
            case "TheContractCarriesOnlyThePortableSurface":
                TheContractCarriesOnlyThePortableSurface = outcome;
                break;
            case "AHostReadsTheDescriptorThroughTheProvider":
                AHostReadsTheDescriptorThroughTheProvider = outcome;
                break;
            case "ThePluginDeclarationSurfaceIsUnchanged":
                ThePluginDeclarationSurfaceIsUnchanged = outcome;
                break;
            case "ThePublishedVersionsStayAvailable":
                ThePublishedVersionsStayAvailable = outcome;
                break;
        }
    }

    /// <summary>The provider of the acceptance: it answers with the descriptor of its session.</summary>
    private sealed class StubProvider(SessionContentDescriptor descriptor) : ISessionContentProvider
    {
        public Task<SessionContentDescriptor?> GetAsync(Guid sessionId, CancellationToken cancellationToken = default)
            => Task.FromResult<SessionContentDescriptor?>(sessionId == descriptor.SessionId ? descriptor : null);
    }
}
