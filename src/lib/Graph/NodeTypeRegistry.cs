
namespace LowlandTech.Core.Graph;

public class NodeTypeRegistry : INodeTypeRegistry
{
    private readonly IReadOnlyList<NodeTypeEntry> _entries;

    public NodeTypeRegistry(IEnumerable<INodeTypeDescriptor> descriptors)
    {
        var entries = new List<NodeTypeEntry>();

        foreach (var descriptor in descriptors)
        {
            var d = descriptor;
            entries.Add(new NodeTypeEntry(
                Id: d.Id,
                Key: d.Key,
                Label: d.Label,
                Icon: d.Icon,
                BlockComponent: d.BlockComponent,
                SortOrder: d.SortOrder,
                EntityType: d.EntityType,
                DescriptorType: d.GetType(),
                ExtractId: entity => d.ExtractId(entity),
                ExtractLabel: entity => d.ExtractLabel(entity),
                ExtractParent: entity => d.ExtractParent(entity),
                IsLeaf: d.IsLeaf,
                IsHeader: d.IsHeader,
                IsListable: d.IsListable,
                IsStatic: true,
                NavigationHeaders: d.NavigationHeaders ?? []
            ));
        }

        _entries = entries
            .GroupBy(e => e.Id)
            .Select(group =>
            {
                var bound = group.Where(e => e.EntityType is not null).ToList();
                if (bound.Count > 1)
                    throw new InvalidOperationException(
                        $"Multiple INodeTypeDescriptor implementations with Id={group.Key} declare an EntityType: " +
                        string.Join(", ", bound.Select(e => e.DescriptorType.FullName)));
                return bound.Count == 1 ? bound[0] : group.First();
            })
            .OrderBy(e => e.SortOrder)
            .ToList();
    }

    public IReadOnlyList<NodeTypeEntry> All => _entries;

    public NodeTypeEntry? FindByKey(string key) =>
        All.FirstOrDefault(e => e.Key == key);

    public NodeTypeEntry? FindById(Guid id) =>
        All.FirstOrDefault(e => e.Id == id);

    public NodeTypeEntry? FindByEntityType(Type entityType) =>
        All.FirstOrDefault(e => e.EntityType == entityType);

    public static NodeType ToEntity(NodeTypeEntry entry) => new()
    {
        Id = entry.Id,
        Key = entry.Key,
        Label = entry.Label,
        Icon = entry.Icon,
        BlockComponent = entry.BlockComponent,
        SortOrder = entry.SortOrder,
        IsLeaf = entry.IsLeaf,
        IsHeader = entry.IsHeader,
        IsStatic = entry.IsStatic,
    };
}
