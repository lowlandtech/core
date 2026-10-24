
namespace LowlandTech.Core.Graph;

public class ComponentTypeRegistry : IComponentTypeRegistry
{
    private Dictionary<string, Type> _byKey = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<Guid, Type> _byId = new();
    private readonly object _lock = new();

    public void Initialize(Assembly assembly)
    {
        lock (_lock)
        {
            var byKey = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            var byId = new Dictionary<Guid, Type>();

            var descriptorType = typeof(IComponentTypeDescriptor);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (!type.GetInterfaces().Contains(descriptorType)) continue;

                var nodeTypeId = (Guid)type.GetProperty(nameof(IComponentTypeDescriptor.NodeTypeId))!.GetValue(null)!;
                var nodeTypeKey = (string)type.GetProperty(nameof(IComponentTypeDescriptor.NodeTypeKey))!.GetValue(null)!;
                var componentType = (Type)type.GetProperty(nameof(IComponentTypeDescriptor.ComponentType))!.GetValue(null)!;

                byKey.TryAdd(nodeTypeKey, componentType);
                byId.TryAdd(nodeTypeId, componentType);
            }

            _byKey = byKey;
            _byId = byId;
        }
    }

    public Type? ResolveByKey(string? key) =>
        key is not null && _byKey.TryGetValue(key, out var type) ? type : null;

    public Type? ResolveByNodeTypeId(Guid nodeTypeId) =>
        _byId.TryGetValue(nodeTypeId, out var type) ? type : null;

    public IReadOnlyDictionary<string, Type> All => _byKey;
}
