
namespace LowlandTech;

/// <summary>
/// Mutable registry of Blazor component assemblies contributed by app plugins.
/// The host reads <see cref="Assemblies"/> when wiring <c>MapRazorComponents</c>
/// so plugins can self-register their Razor pages without the host knowing them
/// at design time.
/// </summary>
public interface IRouteAssemblyRegistry
{
    IReadOnlyList<Assembly> Assemblies { get; }
    void Add(Assembly assembly);
}
