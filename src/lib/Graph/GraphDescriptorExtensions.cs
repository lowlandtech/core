
using System.Diagnostics.CodeAnalysis;

namespace LowlandTech.Core.Graph;

public static class GraphDescriptorExtensions
{
    /// <summary>
    /// Scans <paramref name="assembly"/> for all concrete, public, parameterless-constructor
    /// implementations of <see cref="INodeTypeDescriptor"/> and <see cref="IEdgeTypeDescriptor"/>
    /// and registers each as a DI singleton. Call this from a plugin's ConfigureServices so the
    /// plugin is self-contained and its descriptors are discoverable via IEnumerable&lt;T&gt;.
    /// </summary>
    [RequiresUnreferencedCode(
        "Scanning an assembly for descriptor implementations uses reflection, so the descriptor types are not statically reachable and cannot be preserved by the trimmer.")]
    public static IServiceCollection AddGraphDescriptors(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in SafeGetTypes(assembly))
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (type.GetConstructor(Type.EmptyTypes) is null) continue;

            if (type.IsAssignableTo(typeof(INodeTypeDescriptor)))
                services.AddSingleton(typeof(INodeTypeDescriptor), type);
            else if (type.IsAssignableTo(typeof(IEdgeTypeDescriptor)))
                services.AddSingleton(typeof(IEdgeTypeDescriptor), type);
        }
        return services;
    }

    [RequiresUnreferencedCode(
        "Enumerating the types of an assembly uses reflection, so the descriptor types are not statically reachable and cannot be preserved by the trimmer.")]
    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }
}
