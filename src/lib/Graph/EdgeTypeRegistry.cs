namespace LowlandTech.Core.Graph;

public class EdgeTypeRegistry : IEdgeTypeRegistry
{
    private readonly IReadOnlyList<EdgeTypeEntry> _entries;

    public EdgeTypeRegistry(IEnumerable<IEdgeTypeDescriptor> descriptors, INodeTypeRegistry nodeTypeRegistry)
    {
        var entries = descriptors.Select(d => new EdgeTypeEntry(
            Id: d.Id,
            Key: d.Key,
            Label: d.Label,
            SourceNodeTypeId: d.SourceNodeTypeId,
            TargetNodeTypeId: d.TargetNodeTypeId,
            ClrType: d.ClrType,
            DefaultProperties: d.DefaultProperties,
            DescriptorType: d.GetType(),
            IsStatic: true
        )).ToList();

        foreach (var parentNodeType in nodeTypeRegistry.All)
        {
            foreach (var header in parentNodeType.NavigationHeaders ?? [])
            {
                var childNodeType = nodeTypeRegistry.FindByKey(header.ChildNodeTypeKey);
                if (childNodeType is null) continue;

                var parentToHeaderKey = header.ParentToHeaderEdgeKey
                    ?? NavigationHeaderConventions.ParentToHeaderEdgeKey(parentNodeType.Key, header.NavigationProperty);
                var headerToChildKey = header.HeaderToChildEdgeKey
                    ?? NavigationHeaderConventions.HeaderToChildEdgeKey(header.ChildNodeTypeKey);
                var parentToHeaderDefaults = new Dictionary<string, string>
                {
                    ["required"] = "true",
                    ["label"] = header.HeaderLabel,
                    ["navigationProperty"] = header.NavigationProperty,
                };

                if (!string.IsNullOrWhiteSpace(header.HeaderIcon))
                    parentToHeaderDefaults["icon"] = header.HeaderIcon!;

                AddSyntheticEntry(
                    entries,
                    NavigationHeaderConventions.EdgeTypeId(parentToHeaderKey),
                    parentToHeaderKey,
                    "Contains",
                    parentNodeType.Id,
                    NodeTypes.HeaderNodeType.NodeTypeId,
                    parentToHeaderDefaults);

                AddSyntheticEntry(
                    entries,
                    NavigationHeaderConventions.EdgeTypeId(headerToChildKey),
                    headerToChildKey,
                    "Contains",
                    NodeTypes.HeaderNodeType.NodeTypeId,
                    childNodeType.Id,
                    new Dictionary<string, string>());
            }
        }

        _entries = entries;
    }

    public IReadOnlyList<EdgeTypeEntry> All => _entries;

    public EdgeTypeEntry? FindByKey(string key) =>
        All.FirstOrDefault(e => e.Key == key);

    public EdgeTypeEntry? FindById(Guid id) =>
        All.FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<EdgeTypeEntry> FindRequired(Guid sourceNodeTypeId) =>
        All.Where(e =>
            e.SourceNodeTypeId == sourceNodeTypeId &&
            e.DefaultProperties.TryGetValue("required", out var v) &&
            v == "true")
        .ToList();

    public IReadOnlyList<EdgeTypeEntry> FindBySourceNodeTypeId(Guid sourceNodeTypeId) =>
        All.Where(e => e.SourceNodeTypeId == sourceNodeTypeId).ToList();

    public IReadOnlyList<EdgeTypeEntry> FindByTargetNodeTypeId(Guid targetNodeTypeId) =>
        All.Where(e => e.TargetNodeTypeId == targetNodeTypeId).ToList();

    private static void AddSyntheticEntry(
        List<EdgeTypeEntry> entries,
        Guid id,
        string key,
        string label,
        Guid sourceNodeTypeId,
        Guid targetNodeTypeId,
        IReadOnlyDictionary<string, string> defaultProperties)
    {
        if (entries.Any(e => e.Id == id || string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase)))
            return;

        entries.Add(new EdgeTypeEntry(
            Id: id,
            Key: key,
            Label: label,
            SourceNodeTypeId: sourceNodeTypeId,
            TargetNodeTypeId: targetNodeTypeId,
            ClrType: null,
            DefaultProperties: defaultProperties,
            DescriptorType: typeof(EdgeTypeRegistry),
            IsStatic: true));
    }
}
