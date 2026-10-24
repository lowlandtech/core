namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Installs plugins by starting an external container from the plugin's sidecar
/// image and registering an iframe route that proxies to the container's endpoint.
/// Does not invoke <see cref="IApiPlugin"/> lifecycle hooks in the host process.
/// SPEC2050 US20 SC03.
/// </summary>
public sealed class SidecarInstaller : IPluginInstaller
{
    private readonly IContainerRuntime? _container;
    private readonly IIframeRouteRegistry? _iframeRoutes;
    private readonly IDomainEventBus? _eventBus;

    /// <summary>
    /// Parameterless constructor: supports <see cref="PlanAsync"/> only.
    /// Use the full constructor when <see cref="ExecuteAsync"/> is required.
    /// </summary>
    public SidecarInstaller() { }

    public SidecarInstaller(
        IContainerRuntime container,
        IIframeRouteRegistry iframeRoutes,
        IDomainEventBus eventBus)
    {
        _container = container;
        _iframeRoutes = iframeRoutes;
        _eventBus = eventBus;
    }

    public PluginInstallMode Mode => PluginInstallMode.Sidecar;

    public Task<InstallPlan> PlanAsync(PluginRef package, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(package);

        var steps = new[]
        {
            new InstallStep("start sidecar container",
                "Pull the sidecar image for the plugin from the configured registry and start a container, waiting for its health check to go green."),
            new InstallStep("register iframe route",
                "Add an entry to IIframeRouteRegistry mapping the plugin's UI path to the running container's health endpoint so the main app can embed it.")
        };

        var plan = new InstallPlan(PluginInstallMode.Sidecar, package, steps);
        return Task.FromResult(plan);
    }

    /// <summary>
    /// Step 1: call <see cref="IContainerRuntime.StartAsync"/> â€” waits for the health check.
    /// Step 2: register the iframe route in <see cref="IIframeRouteRegistry"/>.
    /// Step 3: emit <see cref="SidecarInstalled"/>.
    /// NOTE: <see cref="IApiPlugin"/> lifecycle hooks are NOT invoked (spec requirement).
    /// SPEC2050 US20 SC03.
    /// </summary>
    public async Task<InstallResult> ExecuteAsync(InstallPlan plan, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (_container is null || _iframeRoutes is null || _eventBus is null)
            throw new InvalidOperationException(
                "SidecarInstaller.ExecuteAsync requires IContainerRuntime, IIframeRouteRegistry, and IDomainEventBus. " +
                "Use the full constructor.");

        // Step 1 â€” start container and wait for health check (phase: starting-container â†’ healthy).
        var startResult = await _container.StartAsync(plan.Package, ct).ConfigureAwait(false);

        // Step 2 â€” register the iframe route (UI path defaults to "/{pluginId}").
        var uiPath = $"/{plan.Package.PluginId}";
        _iframeRoutes.Register(plan.Package.PluginId, uiPath, startResult.ContainerEndpoint);

        // Step 3 â€” emit SidecarInstalled (phase: healthy).
        await _eventBus.EmitAsync(
            new SidecarInstalled(plan.Package.PluginId, startResult.ContainerEndpoint), ct)
            .ConfigureAwait(false);

        return new InstallResult(
            Succeeded: true,
            SidecarContainerId: startResult.ContainerId);
    }
}
