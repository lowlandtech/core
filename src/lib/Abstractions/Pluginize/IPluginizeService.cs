namespace LowlandTech.Core.Abstractions.Pluginize;

/// <summary>
/// Surface-agnostic pluginize pipeline. Can be called from CLI, MCP, API, or chat dashboard.
/// SPEC2300 US30.
/// </summary>
public interface IPluginizeService
{
    Task<PluginizeResult> RunAsync(PluginizeRequest request, CancellationToken ct = default);
}
