
namespace LowlandTech.Core.Plugins.HotReload;

/// <summary>
/// Default implementation of <see cref="IHotReloadService"/>.
/// Orchestrates: detect (poll or webhook) → stage → suspend/swap/resume → rollback.
/// SPEC2050 US05.
/// </summary>
public sealed class HotReloadService : IHotReloadService
{
    private readonly IPluginRegistry _plugins;
    private readonly IPluginFeed _feed;
    private readonly IVersionStager _stager;
    private readonly IAlcLoader _alcLoader;
    private readonly IDomainEventBus _eventBus;
    private readonly HotReloadConfig _config;

    // Mutable sessions keyed by pluginId (case-insensitive).
    private readonly ConcurrentDictionary<string, HotReloadSession> _sessions =
        new(StringComparer.OrdinalIgnoreCase);

    // Track the currently-loaded IApiPlugin per plugin (the live instance).
    private readonly ConcurrentDictionary<string, IApiPlugin> _livePackages =
        new(StringComparer.OrdinalIgnoreCase);

    public HotReloadService(
        IPluginRegistry plugins,
        IPluginFeed feed,
        IVersionStager stager,
        IAlcLoader alcLoader,
        IDomainEventBus eventBus,
        HotReloadConfig? config = null)
    {
        _plugins = plugins;
        _feed = feed;
        _stager = stager;
        _alcLoader = alcLoader;
        _eventBus = eventBus;
        _config = config ?? new HotReloadConfig();
    }

    /// <inheritdoc />
    public HotReloadSession GetSession(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        return _sessions.TryGetValue(pluginId, out var s) ? s : new HotReloadSession { PluginId = pluginId };
    }

    /// <summary>Register the live plugin package so ApplyAsync can call SuspendAsync / ResumeAsync on it.</summary>
    public void RegisterLivePackage(string pluginId, IApiPlugin package)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        ArgumentNullException.ThrowIfNull(package);
        _livePackages[pluginId] = package;
    }

    /// <inheritdoc />
    public async Task<bool> PollAsync(string pluginId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        var installed = _plugins.Find(pluginId);
        if (installed is null) return false;

        var feedEntries = await _feed.ListAsync(ct).ConfigureAwait(false);
        var feedEntry = feedEntries.FirstOrDefault(e =>
            string.Equals(e.Id, pluginId, StringComparison.OrdinalIgnoreCase));

        if (feedEntry is null) return false;

        var feedVersion = feedEntry.Version;

        // SC10: filter pre-release unless opt-in
        if (!_config.AllowPrerelease && IsPrerelease(feedVersion))
        {
            UpdateSession(pluginId, s => s with { Phase = HotReloadPhase.Idle });
            return false;
        }

        // SC12: block downgrade
        if (!_config.AllowDowngrade && IsDowngrade(installed.Version, feedVersion))
        {
            var current = installed.Version;
            await _eventBus.EmitAsync(new DowngradeRejected(pluginId, feedVersion, current), ct).ConfigureAwait(false);
            return false;
        }

        // SC03: no event when already on latest
        if (string.Equals(installed.Version, feedVersion, StringComparison.OrdinalIgnoreCase))
        {
            // Phase stays Idle
            return false;
        }

        if (!IsNewer(installed.Version, feedVersion))
            return false;

        // New version detected — SC01
        UpdateSession(pluginId, s => s with
        {
            Phase = HotReloadPhase.Detected,
            FromVersion = installed.Version,
            ToVersion = feedVersion,
        });

        await _eventBus.EmitAsync(new UpdateAvailable(pluginId, feedVersion), ct).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> OnWebhookAsync(string pluginId, string version, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        ArgumentNullException.ThrowIfNull(version);

        var installed = _plugins.Find(pluginId);
        var installedVersion = installed?.Version ?? string.Empty;

        // SC11: filter pre-release unless opt-in
        if (!_config.AllowPrerelease && IsPrerelease(version))
            return false;

        // SC12: block downgrade
        if (!_config.AllowDowngrade && installed is not null && IsDowngrade(installedVersion, version))
        {
            await _eventBus.EmitAsync(new DowngradeRejected(pluginId, version, installedVersion), ct).ConfigureAwait(false);
            return false;
        }

        // SC02: webhook immediately triggers detection
        UpdateSession(pluginId, s => s with
        {
            Phase = HotReloadPhase.Detected,
            FromVersion = installedVersion,
            ToVersion = version,
        });

        await _eventBus.EmitAsync(new UpdateAvailable(pluginId, version), ct).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> StageAsync(string pluginId, string version, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        ArgumentNullException.ThrowIfNull(version);

        // SC08: preserve old version assemblies by keeping current staging intact
        // Populate FromVersion from the registry if not already set (when staging is called directly,
        // without a prior PollAsync/OnWebhookAsync detecting the update).
        var currentDescriptor = _plugins.Find(pluginId);
        var fromVersion = currentDescriptor?.Version ?? string.Empty;
        UpdateSession(pluginId, s => s with
        {
            Phase = HotReloadPhase.Staging,
            FromVersion = s.FromVersion ?? (string.IsNullOrEmpty(fromVersion) ? null : fromVersion),
            ToVersion = version,
            PreviousVersionPreserved = true,
        });

        try
        {
            var stagingPath = await _stager.StageAsync(pluginId, version, ct).ConfigureAwait(false);

            UpdateSession(pluginId, s => s with
            {
                Phase = HotReloadPhase.Staged,
                StagingPath = stagingPath,
            });

            await _eventBus.EmitAsync(new VersionStaged(pluginId, stagingPath), ct).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            UpdateSession(pluginId, s => s with
            {
                Phase = HotReloadPhase.Error,
                Error = ex.Message,
            });
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ApplyAsync(string pluginId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        var session = GetSession(pluginId);
        if (session.Phase != HotReloadPhase.Staged || session.StagingPath is null || session.ToVersion is null)
            throw new InvalidOperationException(
                $"Plugin '{pluginId}' must be in Staged phase before Apply. Current: {session.Phase}");

        var fromVersion = session.FromVersion ?? string.Empty;
        var toVersion = session.ToVersion;
        var stagingPath = session.StagingPath;

        // Step 1 — SC05 UAC01: suspend the old package
        UpdateSession(pluginId, s => s with { Phase = HotReloadPhase.Suspending });
        var oldPackage = _livePackages.TryGetValue(pluginId, out var p) ? p : null;
        if (oldPackage is not null)
            await oldPackage.SuspendAsync(ct).ConfigureAwait(false);

        string oldAlcName;
        string newAlcName;
        IApiPlugin newPackage;

        // Step 2 — SC05 UAC02: load new ALC
        UpdateSession(pluginId, s => s with { Phase = HotReloadPhase.Swapping });
        try
        {
            (newAlcName, newPackage) = await _alcLoader.LoadAsync(pluginId, stagingPath, ct).ConfigureAwait(false);
            oldAlcName = session.OldAlcName ?? $"{pluginId}@{fromVersion}";
            UpdateSession(pluginId, s => s with
            {
                OldAlcName = oldAlcName,
                NewAlcName = newAlcName,
            });
        }
        catch (Exception ex)
        {
            // ALC load failed — rollback
            await RollbackCoreAsync(pluginId, ex.Message, oldPackage, fromVersion, ct).ConfigureAwait(false);
            return false;
        }

        // Step 3 — SC05 UAC03/04: initialize and resume the new package
        UpdateSession(pluginId, s => s with { Phase = HotReloadPhase.Resuming });
        try
        {
            // InitializeAsync is modeled as InstallAsync with a null context for hot-reload
            // purposes; the service does not run migrations again — that is US08's domain.
            // For now we call ResumeAsync directly as the spec models the swap as:
            // InitializeAsync → ResumeAsync.
            await newPackage.InstallAsync(new NullInstallContext(pluginId), ct).ConfigureAwait(false);
            await newPackage.ResumeAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Resume failed — unload new ALC and rollback — SC07
            _alcLoader.Unload(newAlcName);
            await RollbackCoreAsync(pluginId, ex.Message, oldPackage, fromVersion, ct).ConfigureAwait(false);
            return false;
        }

        // Step 4 — SC05 UAC05: unload old ALC
        oldAlcName = GetSession(pluginId).OldAlcName ?? $"{pluginId}@{fromVersion}";
        _alcLoader.Unload(oldAlcName);

        // Update live package and registry
        _livePackages[pluginId] = newPackage;
        _plugins.Unregister(pluginId);
        _plugins.Register(newPackage.Descriptor);

        // SC05 UAC06
        UpdateSession(pluginId, s => s with
        {
            Phase = HotReloadPhase.Applied,
            FromVersion = fromVersion,
            ToVersion = toVersion,
            OldAlcName = oldAlcName,
            NewAlcName = newAlcName,
        });

        // SC05 UAC07
        await _eventBus.EmitAsync(
            new UpdateApplied(pluginId, fromVersion, toVersion, oldAlcName, newAlcName), ct)
            .ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task RollbackAsync(string pluginId, string reason, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        var oldPackage = _livePackages.TryGetValue(pluginId, out var p) ? p : null;
        var fromVersion = GetSession(pluginId).FromVersion ?? string.Empty;
        await RollbackCoreAsync(pluginId, reason, oldPackage, fromVersion, ct).ConfigureAwait(false);
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private async Task RollbackCoreAsync(
        string pluginId,
        string reason,
        IApiPlugin? oldPackage,
        string fromVersion,
        CancellationToken ct)
    {
        var session = GetSession(pluginId);

        if (oldPackage is null && string.IsNullOrEmpty(fromVersion))
        {
            // SC09: no previous version — cannot recover
            UpdateSession(pluginId, s => s with
            {
                Phase = HotReloadPhase.Error,
                Error = reason,
            });
            await _eventBus.EmitAsync(
                new UpdateFailed(pluginId, session.ToVersion ?? string.Empty, reason), ct)
                .ConfigureAwait(false);
            return;
        }

        UpdateSession(pluginId, s => s with { Phase = HotReloadPhase.RollingBack });

        try
        {
            // SC07 UAC03: resume the old package
            if (oldPackage is not null)
                await oldPackage.ResumeAsync(ct).ConfigureAwait(false);

            // SC07 UAC04: version record stays at fromVersion (registry already has it)

            UpdateSession(pluginId, s => s with
            {
                Phase = HotReloadPhase.RolledBack,
                Error = reason,
            });

            await _eventBus.EmitAsync(
                new UpdateFailed(pluginId, session.ToVersion ?? string.Empty, reason), ct)
                .ConfigureAwait(false);

            await _eventBus.EmitAsync(
                new RollbackCompleted(pluginId, fromVersion), ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateSession(pluginId, s => s with
            {
                Phase = HotReloadPhase.Error,
                Error = $"Rollback itself failed: {ex.Message}",
            });

            await _eventBus.EmitAsync(
                new UpdateFailed(pluginId, session.ToVersion ?? string.Empty, $"rollback-failed: {ex.Message}"), ct)
                .ConfigureAwait(false);
        }
    }

    private void UpdateSession(string pluginId, Func<HotReloadSession, HotReloadSession> updater)
    {
        _sessions.AddOrUpdate(
            pluginId,
            _ => updater(new HotReloadSession { PluginId = pluginId }),
            (_, existing) => updater(existing));
    }

    private static bool IsPrerelease(string version)
        => version.Contains('-', StringComparison.Ordinal);

    private static bool IsDowngrade(string installed, string candidate)
    {
        if (!TryParseVersion(installed, out var iv) || !TryParseVersion(candidate, out var cv))
            return false;
        return cv < iv;
    }

    private static bool IsNewer(string installed, string candidate)
    {
        if (!TryParseVersion(installed, out var iv) || !TryParseVersion(candidate, out var cv))
            return false;
        return cv > iv;
    }

    private static bool TryParseVersion(string raw, out Version version)
    {
        var dash = raw.IndexOf('-');
        var stripped = dash >= 0 ? raw[..dash] : raw;
        return Version.TryParse(stripped, out version!);
    }

    // Minimal install context used during hot-reload InitializeAsync call.
    private sealed class NullInstallContext(string pluginId) : IPluginInstallContext
    {
        public IServiceProvider Services { get; } = EmptyServiceProvider.Instance;
        public string PluginId { get; } = pluginId;
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static readonly EmptyServiceProvider Instance = new();
        public object? GetService(Type serviceType) => null;
    }
}
