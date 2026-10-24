namespace LowlandTech.Core.Plugins.Contributions;

/// <summary>
/// A plugin contributes zero or more <typeparamref name="TContribution"/>
/// values to the host during <c>LoadPluginsAsync</c>. The host collects
/// contributions from every plugin in dependency order and registers them
/// in a single pass.
/// SPEC2050 — template for US11 endpoint groups, US12 auth schemes,
/// US13 authorization policies, US10 feature capabilities, US06 nav entries.
/// </summary>
public interface IContributes<TContribution>
{
    IEnumerable<TContribution> GetContributions();
}
