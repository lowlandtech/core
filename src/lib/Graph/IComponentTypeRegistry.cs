namespace LowlandTech.Core.Graph;

/// <summary>
/// Resolves Blazor component types from NodeType keys or IDs.
/// Discovers IComponentTypeDescriptor implementations from registered assemblies.
/// Registered as a singleton in DI.
/// </summary>
public interface IComponentTypeRegistry
{
    Type? ResolveByKey(string? key);
    Type? ResolveByNodeTypeId(Guid nodeTypeId);
    IReadOnlyDictionary<string, Type> All { get; }
    void Initialize(System.Reflection.Assembly assembly);
}
