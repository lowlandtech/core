namespace LowlandTech.Core.Plugins;

/// <summary>
/// Identifies the DbContext owned by a plugin and the entity types that live in its schema.
/// SPEC2050 US02.
/// </summary>
public sealed record PluginContextDescriptor(
    string PluginId,
    string SchemaName,
    Type ContextType,
    IReadOnlyList<Type> EntityTypes);
