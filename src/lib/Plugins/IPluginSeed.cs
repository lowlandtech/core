namespace LowlandTech.Core.Plugins;

/// <summary>
/// One unit of first-run / upgrade seed data for a plugin.
/// SPEC2050 US15.
/// </summary>
public interface IPluginSeed
{
    /// <summary>Stable identifier unique within the plugin.</summary>
    string SeedId { get; }

    /// <summary>First plugin version that introduced this seed.</summary>
    Version IntroducedIn { get; }

    /// <summary>Apply the seed in an idempotent, transactional way.</summary>
    Task ApplyAsync(IPluginSeedContext context, CancellationToken cancellationToken);

    /// <summary>Remove data and metadata introduced by this seed.</summary>
    Task RemoveAsync(IPluginSeedContext context, CancellationToken cancellationToken);
}
