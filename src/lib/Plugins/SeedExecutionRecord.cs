namespace LowlandTech.Core.Plugins;

public sealed record SeedExecutionRecord(
    string PluginId,
    string SeedId,
    Version AppliedAtVersion,
    DateTimeOffset AppliedAt);
