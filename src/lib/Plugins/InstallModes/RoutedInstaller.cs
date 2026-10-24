namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Installs plugins by opening a PR that adds the NuGet ref + compose route,
/// waits for CI to build a new image, and switches compose traffic to it.
/// Does not mutate the running process.
/// SPEC2050 US20 SC01 (happy path) + SC05 (build-failure rollback).
/// </summary>
public sealed class RoutedInstaller : IPluginInstaller
{
    private readonly ICiWebhookReceiver? _ci;
    private readonly IComposeSwitch? _compose;
    private readonly IDomainEventBus? _eventBus;

    /// <summary>
    /// Parameterless constructor: supports <see cref="PlanAsync"/> only.
    /// Use the full constructor when <see cref="ExecuteAsync"/> is required.
    /// </summary>
    public RoutedInstaller() { }

    public RoutedInstaller(
        ICiWebhookReceiver ci,
        IComposeSwitch compose,
        IDomainEventBus eventBus)
    {
        _ci = ci;
        _compose = compose;
        _eventBus = eventBus;
    }

    public PluginInstallMode Mode => PluginInstallMode.Routed;

    public Task<InstallPlan> PlanAsync(PluginRef package, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(package);

        var steps = new[]
        {
            new InstallStep("open PR",
                $"Open a pull request that adds the NuGet reference for {package.PluginId} {package.Version} and the compose route for the new image."),
            new InstallStep("wait for build",
                "Wait for the CI pipeline to build and publish the new container image."),
            new InstallStep("switch compose route",
                "Switch the compose traffic routing to point at the newly built image.")
        };

        var plan = new InstallPlan(PluginInstallMode.Routed, package, steps);
        return Task.FromResult(plan);
    }

    /// <summary>
    /// Step 1: emit <see cref="InstallStarted"/> and set phase to awaiting-build.
    /// Step 2: poll <see cref="ICiWebhookReceiver"/> for the CI result.
    /// Step 3a (success): call <see cref="IComposeSwitch.SwitchAsync"/>, emit <see cref="RoutedInstalled"/>.
    /// Step 3b (failure SC05): call <see cref="IComposeSwitch.RevertAsync"/>, emit <see cref="InstallRolledBack"/>.
    /// </summary>
    public async Task<InstallResult> ExecuteAsync(InstallPlan plan, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (_ci is null || _compose is null || _eventBus is null)
            throw new InvalidOperationException(
                "RoutedInstaller.ExecuteAsync requires ICiWebhookReceiver, IComposeSwitch, and IDomainEventBus. " +
                "Use the full constructor.");

        // Step 1 — open PR: emit InstallStarted, phase = awaiting-build.
        var prUrl = $"https://github.com/lowlandtech/foundry/pull/{plan.Package.PluginId}/{plan.Package.Version}";
        await _eventBus.EmitAsync(new InstallStarted(plan.Package.PluginId, plan.Package.Version, prUrl), ct)
            .ConfigureAwait(false);

        // Step 2 — await CI.
        var buildResult = await _ci.WaitForBuildAsync(plan.Package, ct).ConfigureAwait(false);

        if (!buildResult.Succeeded)
        {
            // SC05: build failed — revert compose, emit rollback.
            await _compose.RevertAsync(plan.Package, ct).ConfigureAwait(false);
            var reason = buildResult.ErrorReason ?? "build-failed";
            await _eventBus.EmitAsync(new InstallRolledBack(plan.Package.PluginId, reason), ct)
                .ConfigureAwait(false);
            return new InstallResult(Succeeded: false, PullRequestUrl: prUrl, Error: reason);
        }

        // Step 3 — CI succeeded: switch compose traffic.
        var imageTag = buildResult.ImageTag ?? $"{plan.Package.PluginId}:{plan.Package.Version}";
        await _compose.SwitchAsync(plan.Package, imageTag, ct).ConfigureAwait(false);
        await _eventBus.EmitAsync(new RoutedInstalled(plan.Package.PluginId, plan.Package.Version, imageTag), ct)
            .ConfigureAwait(false);

        return new InstallResult(
            Succeeded: true,
            PullRequestUrl: prUrl,
            DeployedImageTag: imageTag);
    }
}
