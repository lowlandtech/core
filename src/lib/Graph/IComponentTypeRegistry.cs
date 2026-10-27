using System.Diagnostics.CodeAnalysis;

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
    /// <summary>
    /// Discovers the component types published by <paramref name="assembly"/>.
    /// </summary>
    [RequiresUnreferencedCode(
        "Scanning an assembly for descriptor implementations uses reflection, so the descriptor types and their static properties are not statically reachable and cannot be preserved by the trimmer.")]
    void Initialize(System.Reflection.Assembly assembly);
}
