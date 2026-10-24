
namespace LowlandTech;

/// <inheritdoc cref="IRouteAssemblyRegistry"/>
public class RouteAssemblyRegistry : IRouteAssemblyRegistry
{
    private readonly List<Assembly> _assemblies = [];

    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    public void Add(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
            _assemblies.Add(assembly);
    }
}
