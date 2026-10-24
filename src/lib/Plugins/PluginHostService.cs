
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default in-process <see cref="IPluginHostService"/> wiring all the registries shipped
/// in SPEC2050 US01–US15 into a single Install / MakeAvailable / Suspend / Resume / Uninstall
/// pipeline. Supports opt-in <see cref="IAlcLoader"/> isolation (US06 / US08 SC10) and
/// HTTP endpoint introspection via <see cref="IEndpointGroupRegistry"/> (US08 SC11).
/// SPEC2050 US08.
/// </summary>
public sealed class PluginHostService(
    IPluginRegistry plugins,
    IPluginFeed feed,
    IFeatureRegistry features,
    IJobRegistry jobs,
    IEventRouter events,
    IMcpToolRegistry mcpTools,
    IPageTypeRegistry pages,
    IBlockTypeRegistry blocks,
    IPermissionRegistry permissions,
    IResourceRegistry resources,
    IPluginEdgeTypeRegistry edgeTypes,
    IPluginContextRegistry contexts,
    IPluginDependencyResolver dependencyResolver,
    IPluginSeedHistory seedHistory,
    PluginSeedRunner seedRunner,
    PluginHostState hostState,
    IServiceCollection serviceCollection,
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IApiPluginSource? packageSource = null,
    IAuthenticationSchemeRegistry? authenticationSchemes = null,
    IAuthorizationPolicyRegistry? authorizationPolicies = null,
    IEndpointGroupRegistry? endpointGroups = null,
    IDomainEventBus? eventBus = null,
    IAlcLoader? alcLoader = null) : IPluginHostService
{
    /// <summary>
    /// Tracks ALC names per plugin when <see cref="IAlcLoader"/> is registered.
    /// Key = pluginId, Value = alcName returned by <see cref="IAlcLoader.LoadAsync"/>.
    /// SPEC2050 US06 / US08 SC10.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _loadedAlcs = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Discovers plugin packages via the injected <see cref="IApiPluginSource"/>,
    /// sorts them by dependency order (US14), and invokes each plugin's boot lifecycle
    /// hooks — <see cref="IApiPlugin.ConfigureServices"/>,
    /// <see cref="IApiPlugin.ConfigureAuthentication"/> (US12),
    /// <see cref="IApiPlugin.ConfigureAuthorization"/> (US13), and
    /// <see cref="IApiPlugin.ConfigurePermissions"/> (US09) — before <c>app.Build()</c>.
    /// Each plugin's descriptor is registered in <see cref="IPluginRegistry"/>.
    /// <see cref="IApiPlugin.Features"/> are deferred to <see cref="MakeAvailableAsync"/>
    /// so they land after the service provider exists.
    /// </summary>
    public async Task LoadPluginsAsync(IServiceCollection services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (packageSource is null) return;

        var discovered = await packageSource.DiscoverAsync(cancellationToken).ConfigureAwait(false);
        if (discovered.Count == 0) return;

        var byId = discovered.ToDictionary(p => p.Descriptor.Id, StringComparer.OrdinalIgnoreCase);
        var sorted = dependencyResolver.SortByLoadOrder(discovered.Select(p => p.Descriptor));
        var loadedIds = new List<string>();

        foreach (var descriptor in sorted)
        {
            var package = byId[descriptor.Id];

            // US06 / US08 SC10 — opt-in ALC isolation. When IAlcLoader is registered,
            // load the plugin's assembly into an isolated AssemblyLoadContext so its
            // private dependencies stay inside the plugin boundary and crashes in one
            // plugin cannot propagate to peer plugins or the host.
            if (alcLoader is not null && descriptor.AssemblyPath is { Length: > 0 } asmPath)
            {
                var (alcName, alcPackage) = await alcLoader.LoadAsync(descriptor.Id, asmPath, cancellationToken).ConfigureAwait(false);
                _loadedAlcs[descriptor.Id] = alcName;
                // Use the package returned by the ALC loader (it was resolved from the isolated context).
                package = alcPackage;
                byId[descriptor.Id] = alcPackage;
            }

            package.ConfigureServices(services, configuration);
            if (authenticationSchemes is not null)
                package.ConfigureAuthentication(authenticationSchemes, configuration);
            if (authorizationPolicies is not null)
                package.ConfigureAuthorization(authorizationPolicies);
            package.ConfigurePermissions(permissions, resources);

            // Register the descriptor from the active package. When an ALC loader
            // replaced the package, the ALC-loaded package's descriptor is used so
            // the registry reflects the isolated package's identity.
            plugins.Register(package.Descriptor);

            // US10 / US11: drain the plugin's feature packages so endpoint groups,
            // background jobs, MCP tools, pages, and blocks land in their registries
            // before app.Build(). Runs inline (not via MakeAvailableAsync) so we
            // don't try to Start jobs against a service provider that isn't built yet.
            if (package.Features.Count > 0)
            {
                var builder = new FeatureRegistrationBuilder(
                    features,
                    descriptor.Id,
                    pageRegistry: pages,
                    blockRegistry: blocks,
                    endpointRegistry: endpointGroups,
                    jobRegistry: jobs,
                    toolRegistry: mcpTools,
                    eventRouter: events,
                    edgeTypeRegistry: edgeTypes,
                    services: services);
                foreach (var featurePackage in package.Features)
                    featurePackage.Register(builder);
            }

            hostState.Set(descriptor.Id, PluginInstallState.Installed);
            loadedIds.Add(descriptor.Id);
        }

        // SPEC2050 US07 — register IFeatureSettingsProvider<T> for every feature that
        // declared a SettingsType.  A single shared IFeatureSettingsStore (in-memory) is
        // registered as a singleton; production hosts replace it with a DB-backed store.
        RegisterFeatureSettingsProviders(services);

        // SPEC2050 US14 SC01 UAC05 — announce host boot completion so subscribers
        // (tests, diagnostics, startup dashboards) can observe the final plugin set.
        if (eventBus is not null)
            await eventBus.EmitAsync(new PluginsLoaded(loadedIds), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Registers <see cref="IFeatureSettingsProvider{TSettings}"/> and
    /// <see cref="IFeatureSettingsStore"/> into <paramref name="services"/> for every
    /// feature whose <see cref="FeatureDescriptor.SettingsType"/> is non-null.
    /// SPEC2050 US07.
    /// </summary>
    private void RegisterFeatureSettingsProviders(IServiceCollection services)
    {
        var featuresWithSettings = features.All.Where(f => f.SettingsType is not null).ToList();
        if (featuresWithSettings.Count == 0) return;

        // Register one shared in-memory store (idempotent: skip if already registered).
        if (services.All(d => d.ServiceType != typeof(IFeatureSettingsStore)))
            services.AddSingleton<IFeatureSettingsStore, InMemoryFeatureSettingsStore>();

        var providerOpenType = typeof(FeatureSettingsProvider<>);
        var providerInterfaceOpenType = typeof(IFeatureSettingsProvider<>);

        foreach (var feature in featuresWithSettings)
        {
            var settingsType = feature.SettingsType!;
            var closedInterface = providerInterfaceOpenType.MakeGenericType(settingsType);
            var closedImpl = providerOpenType.MakeGenericType(settingsType);
            var featureId = feature.Id;

            // Idempotent: skip if already registered (e.g. by ConfigureServices).
            if (services.Any(d => d.ServiceType == closedInterface)) continue;

            services.AddSingleton(closedInterface, sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var store = sp.GetRequiredService<IFeatureSettingsStore>();
                return Activator.CreateInstance(closedImpl, featureId, cfg, store)!;
            });
        }
    }

    public async Task<IReadOnlyList<PluginSummary>> DisplayAsync(CancellationToken cancellationToken = default)
    {
        var feedEntries = await feed.ListAsync(cancellationToken).ConfigureAwait(false);
        var installed = plugins.All.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        var summaries = new List<PluginSummary>();

        // Installed plugins (with optional newer feed version)
        foreach (var p in plugins.All)
        {
            var feedMatch = feedEntries.FirstOrDefault(e => string.Equals(e.Id, p.Id, StringComparison.OrdinalIgnoreCase));
            summaries.Add(new PluginSummary(
                Id: p.Id,
                Name: p.Name,
                InstalledVersion: p.Version,
                State: hostState.Get(p.Id, PluginInstallState.Installed),
                AvailableVersion: feedMatch?.Version));
        }

        // Feed-only plugins
        foreach (var entry in feedEntries)
        {
            if (installed.ContainsKey(entry.Id)) continue;
            summaries.Add(new PluginSummary(
                Id: entry.Id,
                Name: entry.Name,
                InstalledVersion: string.Empty,
                State: PluginInstallState.Available,
                AvailableVersion: entry.Version));
        }

        return summaries;
    }

    public async Task<PluginInstallResult> InstallAsync(IApiPlugin plugin, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        var descriptor = plugin.Descriptor;

        // Idempotency: same id + same version = no-op success.
        var existing = plugins.Find(descriptor.Id);
        if (existing is not null && string.Equals(existing.Version, descriptor.Version, StringComparison.OrdinalIgnoreCase))
        {
            return new PluginInstallResult(
                Success: true,
                PluginId: descriptor.Id,
                Version: descriptor.Version,
                AlreadyInstalled: true);
        }

        // Validate dependencies against currently-installed plugins.
        try
        {
            dependencyResolver.ValidateDependencies(descriptor, plugins.All);
        }
        catch (Exception ex)
        {
            hostState.Set(descriptor.Id, PluginInstallState.Failed);
            return new PluginInstallResult(
                Success: false,
                PluginId: descriptor.Id,
                Version: descriptor.Version,
                FailureReason: ex.Message);
        }

        // For upgrades: unregister the previous version before registering the new one.
        // (Fresh install: existing is null, so no-op.)
        if (existing is not null)
            plugins.Unregister(existing.Id);

        // Register descriptor first so dependent plugins can validate against us.
        try
        {
            plugins.Register(descriptor);
        }
        catch (Exception ex)
        {
            hostState.Set(descriptor.Id, PluginInstallState.Failed);
            return new PluginInstallResult(
                Success: false,
                PluginId: descriptor.Id,
                Version: descriptor.Version,
                FailureReason: ex.Message);
        }

        // Configure DI services.
        try
        {
            plugin.ConfigureServices(serviceCollection, configuration);
            // Rebuild so InstallAsync and seedRunner can resolve newly-registered services
            // (e.g. DbContextFactory registered during ConfigureServices).
            serviceProvider = serviceCollection.BuildServiceProvider();
        }
        catch (Exception ex)
        {
            plugins.Unregister(descriptor.Id);
            hostState.Set(descriptor.Id, PluginInstallState.Failed);
            return new PluginInstallResult(
                Success: false,
                PluginId: descriptor.Id,
                Version: descriptor.Version,
                FailureReason: ex.Message);
        }

        // Run schema migrations + first-install hooks.
        try
        {
            var installContext = new DefaultPluginInstallContext(serviceProvider, descriptor.Id);
            await plugin.InstallAsync(installContext, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            plugins.Unregister(descriptor.Id);
            hostState.Set(descriptor.Id, PluginInstallState.Failed);
            return new PluginInstallResult(
                Success: false,
                PluginId: descriptor.Id,
                Version: descriptor.Version,
                FailureReason: ex.Message);
        }

        // Run seeds (idempotent + version-aware).
        try
        {
            var fromVersion = existing is not null
                ? ParseVersion(existing.Version)
                : new Version(0, 0, 0);
            await seedRunner.RunAsync(plugin, serviceProvider, fromVersion, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Migrations applied but seeds failed: leave the plugin in Failed state for human inspection.
            hostState.Set(descriptor.Id, PluginInstallState.Failed);
            return new PluginInstallResult(
                Success: false,
                PluginId: descriptor.Id,
                Version: descriptor.Version,
                FailureReason: ex.Message);
        }

        hostState.Set(descriptor.Id, PluginInstallState.Installed);
        return new PluginInstallResult(true, descriptor.Id, descriptor.Version);
    }

    public async Task MakeAvailableAsync(string pluginId, IEnumerable<IFeaturePackage> featurePackages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        ArgumentNullException.ThrowIfNull(featurePackages);

        if (plugins.Find(pluginId) is null)
            throw new InvalidOperationException($"Plugin '{pluginId}' is not installed.");

        var builder = new FeatureRegistrationBuilder(
            features,
            pluginId,
            pageRegistry: pages,
            blockRegistry: blocks,
            endpointRegistry: endpointGroups,
            jobRegistry: jobs,
            toolRegistry: mcpTools,
            eventRouter: events,
            edgeTypeRegistry: edgeTypes);
        foreach (var pkg in featurePackages)
            pkg.Register(builder);

        // Start any background jobs already in the registry that belong to this plugin's features.
        var pluginFeatureIds = features.All
            .Where(f => string.Equals(f.PluginId, pluginId, StringComparison.OrdinalIgnoreCase))
            .Select(f => f.Id)
            .ToArray();

        foreach (var job in jobs.All)
        {
            if (pluginFeatureIds.Contains(job.FeatureId, StringComparer.OrdinalIgnoreCase))
                jobs.Start(job.Id);
        }

        hostState.Set(pluginId, PluginInstallState.Installed);

        // US08 SC11 — HTTP endpoint introspection: after patching all registries,
        // emit EndpointsRegistered when the plugin contributed endpoint groups.
        // This lets the host audit, log, or route-refresh after a live plugin load.
        if (eventBus is not null && endpointGroups is not null)
        {
            // Endpoint groups carry a FeatureId, not a PluginId. Look up which
            // features belong to this plugin to find its endpoint groups.
            var pluginEndpointGroupIds = endpointGroups.All
                .Where(g => pluginFeatureIds.Contains(g.FeatureId, StringComparer.OrdinalIgnoreCase))
                .Select(g => g.FeatureId + ":" + g.GroupType.Name)
                .ToArray();

            if (pluginEndpointGroupIds.Length > 0)
            {
                await eventBus.EmitAsync(
                    new EndpointsRegistered(pluginId, pluginEndpointGroupIds),
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public Task SuspendAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        if (plugins.Find(pluginId) is null)
            throw new InvalidOperationException($"Plugin '{pluginId}' is not installed.");

        var pluginFeatureIds = features.All
            .Where(f => string.Equals(f.PluginId, pluginId, StringComparison.OrdinalIgnoreCase))
            .Select(f => f.Id)
            .ToArray();

        foreach (var job in jobs.All)
        {
            if (pluginFeatureIds.Contains(job.FeatureId, StringComparer.OrdinalIgnoreCase))
                jobs.Stop(job.Id);
        }

        hostState.Set(pluginId, PluginInstallState.Suspended);
        return Task.CompletedTask;
    }

    public Task ResumeAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        if (plugins.Find(pluginId) is null)
            throw new InvalidOperationException($"Plugin '{pluginId}' is not installed.");

        var pluginFeatureIds = features.All
            .Where(f => string.Equals(f.PluginId, pluginId, StringComparison.OrdinalIgnoreCase))
            .Select(f => f.Id)
            .ToArray();

        foreach (var job in jobs.All)
        {
            if (pluginFeatureIds.Contains(job.FeatureId, StringComparer.OrdinalIgnoreCase))
                jobs.Start(job.Id);
        }

        hostState.Set(pluginId, PluginInstallState.Installed);
        return Task.CompletedTask;
    }

    public async Task<PluginUninstallResult> UninstallAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        if (plugins.Find(pluginId) is null)
            return new PluginUninstallResult(true, pluginId);

        // Block when other plugins depend on this one.
        var dependents = dependencyResolver.FindDependents(pluginId, plugins.All);
        if (dependents.Count > 0)
        {
            return new PluginUninstallResult(
                Success: false,
                PluginId: pluginId,
                FailureReason: $"Cannot uninstall '{pluginId}': dependents still installed: {string.Join(", ", dependents)}",
                BlockingDependents: dependents);
        }

        // Find every feature contributed by this plugin so we can sweep capability registries.
        var pluginFeatureIds = features.All
            .Where(f => string.Equals(f.PluginId, pluginId, StringComparison.OrdinalIgnoreCase))
            .Select(f => f.Id)
            .ToArray();

        foreach (var featureId in pluginFeatureIds)
        {
            jobs.RemoveByFeature(featureId);
            events.RemoveByFeature(featureId);
            mcpTools.RemoveByFeature(featureId);
            edgeTypes.RemoveByFeature(featureId);
            features.Unregister(featureId);
        }

        // Plugin-level descriptors.
        permissions.RemoveByPlugin(pluginId);
        resources.RemoveByPlugin(pluginId);
        contexts.Unregister(pluginId);
        await seedHistory.RemoveAllForPluginAsync(pluginId, cancellationToken).ConfigureAwait(false);
        plugins.Unregister(pluginId);
        hostState.Remove(pluginId);

        // US06 / US08 SC10 — unload the plugin's AssemblyLoadContext when one was
        // allocated during LoadPluginsAsync. This releases the isolated boundary and
        // allows GC to collect the plugin assemblies.
        if (alcLoader is not null && _loadedAlcs.TryRemove(pluginId, out var alcName))
            alcLoader.Unload(alcName);

        return new PluginUninstallResult(true, pluginId);
    }

    private static Version ParseVersion(string raw)
    {
        var dash = raw.IndexOf('-');
        var stripped = dash >= 0 ? raw[..dash] : raw;
        return Version.Parse(stripped);
    }

    private sealed class DefaultPluginInstallContext(IServiceProvider services, string pluginId) : IPluginInstallContext
    {
        public IServiceProvider Services { get; } = services;
        public string PluginId { get; } = pluginId;
    }
}
