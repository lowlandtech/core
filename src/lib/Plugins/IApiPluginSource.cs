namespace LowlandTech.Core.Plugins;

/// <summary>
/// Supplies <see cref="IApiPlugin"/> instances to the host during
/// <see cref="IPluginHostService.LoadPluginsAsync"/>. Implementations include a
/// static in-memory list (tests, unit-of-work hosts), a filesystem scanner
/// (reads the <c>installed/</c> folder layout), or a DRD-backed source
/// (marketplace-driven installs).
/// SPEC2050 US14.
/// </summary>
public interface IApiPluginSource
{
    Task<IReadOnlyList<IApiPlugin>> DiscoverAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory <see cref="IApiPluginSource"/> that yields a fixed list of
/// plugin packages. Primary use cases: tests and composition-root hosts that
/// hand the full plugin list to the container at startup.
/// </summary>
public sealed class StaticApiPluginSource(IEnumerable<IApiPlugin> packages) : IApiPluginSource
{
    private readonly IReadOnlyList<IApiPlugin> _packages = packages?.ToArray() ?? throw new ArgumentNullException(nameof(packages));

    public Task<IReadOnlyList<IApiPlugin>> DiscoverAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_packages);
}
