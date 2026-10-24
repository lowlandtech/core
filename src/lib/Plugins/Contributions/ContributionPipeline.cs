namespace LowlandTech.Core.Plugins.Contributions;

/// <summary>
/// Collects <typeparamref name="TContribution"/> from every
/// <see cref="IContributes{TContribution}"/> provider and registers them
/// through a caller-supplied sink. The host runs one pipeline per contribution
/// type during <c>LoadPluginsAsync</c>.
/// SPEC2050 — template for the "each plugin contributes, host collects" pattern.
/// </summary>
public sealed class ContributionPipeline<TContribution>
{
    private readonly IEnumerable<IContributes<TContribution>> _providers;

    public ContributionPipeline(IEnumerable<IContributes<TContribution>> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        _providers = providers;
    }

    /// <summary>
    /// Drain every provider's contributions in enumeration order and pass each
    /// to <paramref name="register"/>. Providers are expected to be supplied in
    /// plugin dependency order (see <c>IPluginDependencyResolver.SortByLoadOrder</c>).
    /// </summary>
    public void Run(Action<TContribution> register)
    {
        ArgumentNullException.ThrowIfNull(register);
        foreach (var provider in _providers)
        {
            foreach (var contribution in provider.GetContributions())
                register(contribution);
        }
    }
}
