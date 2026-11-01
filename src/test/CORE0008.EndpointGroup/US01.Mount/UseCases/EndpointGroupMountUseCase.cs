using LowlandTech.Core.Plugins;

namespace LowlandTech.Core.Tests.CORE0008.EndpointGroup.US01.Mount.UseCases;

/// <summary>
/// The acceptance inventory of CORE0008-US01-SC01. The declaration is observed
/// through the canonical contract that the producers and the hosts reference
/// and through the package that the feed carries, so that the observations are
/// of the shipped contract and not of a shape invented by this lane.
/// </summary>
public sealed class EndpointGroupMountUseCase : global::LowlandTech.Core.Tests.Refinement.IUseCase
{
    /// <summary>The Core package generation that was published before this declaration.</summary>
    private const string PublishedCorePackage = "LowlandTech.Core.2026.10.3-20261029175917.nupkg";

    /// <summary>The digest of that published package.</summary>
    private const string PublishedCoreSha256 = "fcc947c0ce06827c9cc27127dad5eebe21254e8e6a541b3623de869ca8857f21";

    /// <summary>The host context that the declaration owns.</summary>
    private const string HostContextType = "LowlandTech.Core.Plugins.IEndpointGroupContext";

    private readonly List<global::LowlandTech.Core.Tests.Refinement.ResultCase> results = [];

    /// <summary>The named result cases of this scenario.</summary>
    public IReadOnlyList<global::LowlandTech.Core.Tests.Refinement.ResultCase> ResultCases => results;

    /// <summary>The canonical contract declares the entry point and its host context.</summary>
    public bool? TheEntryPointIsDeclared { get; private set; }

    /// <summary>The declared metadata of the group stays in the descriptor.</summary>
    public bool? TheMetadataStaysInTheDescriptor { get; private set; }

    /// <summary>A host mounts a declared group through the declared entry point.</summary>
    public bool? AHostMountsTheDeclaredGroup { get; private set; }

    /// <summary>The published Core generation stays available and unchanged.</summary>
    public bool? ThePublishedCoreVersionStaysAvailable { get; private set; }

    /// <summary>The package that the consumers resolve carries the declaration.</summary>
    public bool? TheConsumersResolveTheDeclaration { get; private set; }

    /// <summary>The diagnostic that the measurement of a named result case recorded.</summary>
    public string Diagnostic(string key)
        => results.FirstOrDefault(result => result.Key == key)?.Diagnostic ?? "no diagnostic was recorded";

    public Task ArrangeAsync() => Task.CompletedTask;

    public Task InvokeAsync()
    {
        // The canonical contract declares the entry point and the host context,
        // and it names no web framework type.
        var entryPoint = typeof(global::LowlandTech.Core.Plugins.EndpointGroup).GetMethod("MapEndpoints");
        var contextType = typeof(IEndpointGroupContext);
        var services = contextType.GetProperty("Services");
        var map = contextType.GetMethod("Map");
        var declared = entryPoint is not null
                       && entryPoint.IsAbstract
                       && entryPoint.IsPublic
                       && entryPoint.GetParameters() is [{ ParameterType: var parameter }] single
                       && parameter == contextType
                       && services is not null
                       && services.PropertyType == typeof(IServiceProvider)
                       && map is not null
                       && map.GetParameters().Select(p => p.ParameterType)
                           .SequenceEqual([typeof(string), typeof(string), typeof(Delegate)]);
        Record("TheEntryPointIsDeclared", declared,
            declared
                ? "The canonical contract declares the abstract entry point on the group and the host context it takes, and both name no web framework type."
                : $"The declaration is incomplete: entry point {entryPoint?.ToString() ?? "missing"}, services {services?.PropertyType.ToString() ?? "missing"}, map {map?.ToString() ?? "missing"}.");

        // The metadata of the group stays in the descriptor, and the registry
        // still refuses a conflicting prefix.
        var registry = new EndpointGroupRegistry();
        var group = new SampleGroup();
        registry.Register(new EndpointGroupDescriptor(
            "feature-a", typeof(SampleGroup), group.GroupPrefix, group.Tags,
            group.RequiredAuthenticationScheme, group.RequiredPolicy));
        var descriptor = registry.All.FirstOrDefault(entry => entry.FeatureId == "feature-a");
        var metadata = descriptor is not null
                       && descriptor.GroupPrefix == "/api/sample"
                       && descriptor.Tags.SequenceEqual(["sample"])
                       && descriptor.RequiredAuthenticationScheme == "Bearer"
                       && descriptor.RequiredPolicy == "sample.read";
        var refused = false;
        try
        {
            registry.Register(new EndpointGroupDescriptor(
                "feature-b", typeof(SampleGroup), "/api/sample", [], null, null));
        }
        catch (EndpointPrefixConflictException)
        {
            refused = true;
        }
        var kept = metadata && refused;
        Record("TheMetadataStaysInTheDescriptor", kept,
            kept
                ? "The descriptor carries the prefix, the tags, the scheme and the policy, and the registry refused the conflicting prefix."
                : $"The metadata was kept: {metadata}; the registry refused the conflicting prefix: {refused}.");

        // A host mounts the declared group through the declared entry point.
        var context = new HostContext { Prefix = group.GroupPrefix };
        group.MapEndpoints(context);
        var mounted = context.Routes.Count == 2
                      && context.Routes[0].Method == "GET"
                      && context.Routes[0].Pattern == "/items"
                      && context.Routes[1].Method == "POST"
                      && context.Routes[1].Pattern == "/items"
                      && context.Prefix == group.GroupPrefix
                      && context.Services is not null;
        Record("AHostMountsTheDeclaredGroup", mounted,
            mounted
                ? $"The host mounted the group at {context.Prefix} and received {context.Routes.Count} routes on its own context."
                : $"The host mounted {context.Routes.Count} routes at {context.Prefix}.");

        // The published generation before this declaration stays available.
        var repositoryRoot = global::LowlandTech.Core.Tests.Refinement.GateRunner.RepositoryRoot("verify-closure.py");
        var feedRoot = repositoryRoot is null
            ? null
            : Path.Combine(Directory.GetParent(repositoryRoot)!.FullName, "packages");
        var published = feedRoot is null ? null : Path.Combine(feedRoot, PublishedCorePackage);
        var available = published is not null
                        && File.Exists(published)
                        && global::LowlandTech.Core.Tests.Refinement.PackageFeed.Sha256(published) == PublishedCoreSha256;
        Record("ThePublishedCoreVersionStaysAvailable", available,
            available
                ? $"The published package {PublishedCorePackage} is still in the feed with its recorded digest."
                : $"The published package {PublishedCorePackage} is not in the feed with its recorded digest.");

        // The package that the consumers resolve carries the declaration.
        var resolved = NewestCorePackage(repositoryRoot, feedRoot);
        var types = resolved is null
            ? []
            : global::LowlandTech.Core.Tests.Refinement.PackageFeed.PublicTypes(resolved);
        var carries = types.Contains(HostContextType);
        Record("TheConsumersResolveTheDeclaration", carries,
            carries
                ? $"The Core package {Path.GetFileName(resolved!)} publishes {HostContextType}."
                : $"The Core package {(resolved is null ? "none" : Path.GetFileName(resolved))} does not publish {HostContextType}.");

        return Task.CompletedTask;
    }

    /// <summary>
    /// The Core package that a consumer resolves: the staged candidate of the
    /// release when one exists, and the newest published generation otherwise.
    /// </summary>
    private static string? NewestCorePackage(string? repositoryRoot, string? feedRoot)
    {
        var candidates = new List<(string Path, string Version)>();
        if (repositoryRoot is not null)
        {
            var staging = Path.Combine(repositoryRoot, "artifacts", "local-packages");
            if (Directory.Exists(staging))
            {
                foreach (var directory in Directory.EnumerateDirectories(staging))
                {
                    foreach (var file in Directory.EnumerateFiles(directory, "LowlandTech.Core.*.nupkg"))
                    {
                        var version = VersionOf(file);
                        if (version is not null) candidates.Add((file, version));
                    }
                }
            }
        }
        if (feedRoot is not null && Directory.Exists(feedRoot))
        {
            foreach (var file in Directory.EnumerateFiles(feedRoot, "LowlandTech.Core.*.nupkg"))
            {
                var version = VersionOf(file);
                if (version is not null) candidates.Add((file, version));
            }
        }
        if (candidates.Count == 0) return null;
        var newest = candidates[0];
        foreach (var candidate in candidates.Skip(1))
        {
            if (global::LowlandTech.Core.Tests.Refinement.PackageFeed.ComparePublication(candidate.Version, newest.Version) > 0)
            {
                newest = candidate;
            }
        }
        return newest.Path;
    }

    /// <summary>The version that the file name of a Core package declares.</summary>
    private static string? VersionOf(string path)
    {
        const string prefix = "LowlandTech.Core.";
        var name = Path.GetFileName(path);
        if (!name.StartsWith(prefix, StringComparison.Ordinal)
            || !name.EndsWith(".nupkg", StringComparison.Ordinal)) return null;
        var version = name[prefix.Length..^".nupkg".Length];
        return version.Length > 0 && char.IsDigit(version[0]) ? version : null;
    }

    private void Record(string key, bool outcome, string diagnostic)
    {
        results.RemoveAll(result => result.Key == key);
        results.Add(new global::LowlandTech.Core.Tests.Refinement.ResultCase(
            key, outcome ? "Observed" : "NotObserved", diagnostic));
        switch (key)
        {
            case "TheEntryPointIsDeclared":
                TheEntryPointIsDeclared = outcome;
                break;
            case "TheMetadataStaysInTheDescriptor":
                TheMetadataStaysInTheDescriptor = outcome;
                break;
            case "AHostMountsTheDeclaredGroup":
                AHostMountsTheDeclaredGroup = outcome;
                break;
            case "ThePublishedCoreVersionStaysAvailable":
                ThePublishedCoreVersionStaysAvailable = outcome;
                break;
            case "TheConsumersResolveTheDeclaration":
                TheConsumersResolveTheDeclaration = outcome;
                break;
        }
    }

    /// <summary>The declared group of the acceptance.</summary>
    private sealed class SampleGroup : global::LowlandTech.Core.Plugins.EndpointGroup
    {
        public override string GroupPrefix => "/api/sample";

        public override string[] Tags => ["sample"];

        public override string? RequiredAuthenticationScheme => "Bearer";

        public override string? RequiredPolicy => "sample.read";

        public override void MapEndpoints(IEndpointGroupContext context)
        {
            context.Map("GET", "/items", (Delegate)(Action)(() => { }));
            context.Map("POST", "/items", (Delegate)(Action)(() => { }));
        }
    }

    /// <summary>The host surface of the acceptance: it owns the route table.</summary>
    private sealed class HostContext : IEndpointGroupContext
    {
        /// <summary>The routes that the group mapped on the host.</summary>
        public List<(string Method, string Pattern)> Routes { get; } = [];

        /// <summary>The prefix that the host mounted the group at.</summary>
        public string? Prefix { get; set; }

        public IServiceProvider Services { get; } = new ServiceProviderStub();

        public void Map(string method, string pattern, Delegate handler)
            => Routes.Add((method, pattern));

        private sealed class ServiceProviderStub : IServiceProvider
        {
            public object? GetService(Type serviceType) => null;
        }
    }
}
