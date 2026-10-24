namespace LowlandTech.Core.Plugins;

/// <summary>
/// Declarative description of an MCP tool contributed by a feature, surfaced under
/// <c>/api/mcp</c> when the feature is enabled.
/// SPEC2050 US10.
/// </summary>
public sealed record McpToolDescriptor(
    string Name,
    string FeatureId,
    Type ToolType,
    string? Description = null);
