namespace LowlandTech.Core.Plugins;

/// <summary>
/// Orchestrates seed execution for a plugin: filters by version, checks history,
/// records successes, surfaces failures. Each seed runs in its own logical
/// transaction so a partial failure leaves earlier seeds committed.
/// SPEC2050 US15.
/// </summary>
public sealed class PluginSeedRunner(IPluginSeedHistory history)
{
    public async Task RunAsync(
        IApiPlugin plugin,
        IServiceProvider services,
        Version fromVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(services);

        var toVersion = ParseVersion(plugin.Descriptor.Version);
        var context = new PluginSeedContext(services, plugin.Descriptor.Id, fromVersion, toVersion);

        foreach (var seed in plugin.Seeds)
        {
            // Skip seeds introduced in a future plugin version (defensive â€” a plugin
            // build should never declare such a seed but the check is cheap).
            if (seed.IntroducedIn > toVersion) continue;

            // Skip seeds already applied (idempotent re-install / partial-failure resume).
            if (await history.HasAppliedAsync(plugin.Descriptor.Id, seed.SeedId, cancellationToken))
                continue;

            // Skip seeds that pre-date the previous version (only run on the upgrade boundary).
            if (seed.IntroducedIn <= fromVersion)
                continue;

            await seed.ApplyAsync(context, cancellationToken);
            await history.RecordAsync(new SeedExecutionRecord(
                plugin.Descriptor.Id,
                seed.SeedId,
                toVersion,
                DateTimeOffset.UtcNow), cancellationToken);
        }
    }

    private static Version ParseVersion(string raw)
    {
        var dash = raw.IndexOf('-');
        var stripped = dash >= 0 ? raw[..dash] : raw;
        return Version.Parse(stripped);
    }
}
