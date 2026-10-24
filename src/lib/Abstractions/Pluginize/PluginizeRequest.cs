namespace LowlandTech.Core.Abstractions.Pluginize;

/// <summary>
/// Input for the pluginize pipeline.
/// SPEC2300 US30.
/// </summary>
public record PluginizeRequest
{
    public required string PluginId { get; init; }
    public required string DocsPath { get; init; }
    public required string OutputPath { get; init; }
    public string? ConnectionString { get; init; }
    public string? Only { get; init; }
    public string PageTemplate { get; init; } = "legacy";
    public bool Force { get; init; }
    public bool DryRun { get; init; }
    public bool Verbose { get; init; }
}
