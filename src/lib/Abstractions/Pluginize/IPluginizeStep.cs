namespace LowlandTech.Core.Abstractions.Pluginize;

/// <summary>
/// A discrete step in the pluginize pipeline. Each step transforms an input to an output.
/// SPEC2300 US30.
/// </summary>
public interface IPluginizeStep<TIn, TOut>
{
    Task<TOut> ExecuteAsync(TIn input, CancellationToken ct);
}
