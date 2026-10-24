namespace LowlandTech.Core.Graph;

public static class GraphDescriptorValidator
{
    public static void ValidateOrThrow(INodeTypeRegistry nodeTypeRegistry, IEdgeTypeRegistry edgeTypeRegistry)
    {
        var errors = new List<string>();

        ValidateNodeTypes(nodeTypeRegistry.All, errors);
        ValidateEdgeTypes(edgeTypeRegistry.All, nodeTypeRegistry.All, errors);

        if (errors.Count > 0)
            throw new InvalidOperationException($"Graph descriptor validation failed:{Environment.NewLine}- {string.Join(Environment.NewLine + "- ", errors)}");
    }

    private static void ValidateNodeTypes(IReadOnlyList<NodeTypeEntry> nodeTypes, List<string> errors)
    {
        var duplicateNodeIds = nodeTypes.GroupBy(n => n.Id).Where(g => g.Count() > 1).ToList();
        foreach (var group in duplicateNodeIds)
            errors.Add($"Duplicate NodeType Id '{group.Key}' in: {string.Join(", ", group.Select(x => x.DescriptorType.Name))}");

        var duplicateNodeKeys = nodeTypes.GroupBy(n => n.Key, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).ToList();
        foreach (var group in duplicateNodeKeys)
            errors.Add($"Duplicate NodeType Key '{group.Key}' in: {string.Join(", ", group.Select(x => x.DescriptorType.Name))}");

        var duplicateEntityTypes = nodeTypes
            .Where(n => n.EntityType is not null)
            .GroupBy(n => n.EntityType!)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in duplicateEntityTypes)
            errors.Add($"EntityType '{group.Key.FullName}' is mapped to multiple NodeTypes: {string.Join(", ", group.Select(x => x.Key))}");

        foreach (var nodeType in nodeTypes)
        {
            var duplicateHeaders = (nodeType.NavigationHeaders ?? [])
                .GroupBy(h => h.HeaderLabel, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in duplicateHeaders)
                errors.Add($"NodeType '{nodeType.Key}' declares duplicate navigation header '{group.Key}'.");

            foreach (var header in nodeType.NavigationHeaders ?? [])
            {
                if (!nodeTypes.Any(n => string.Equals(n.Key, header.ChildNodeTypeKey, StringComparison.OrdinalIgnoreCase)))
                    errors.Add($"NodeType '{nodeType.Key}' navigation header '{header.HeaderLabel}' targets unknown child NodeType '{header.ChildNodeTypeKey}'.");
            }
        }
    }

    private static void ValidateEdgeTypes(
        IReadOnlyList<EdgeTypeEntry> edgeTypes,
        IReadOnlyList<NodeTypeEntry> nodeTypes,
        List<string> errors)
    {
        var nodeTypeIds = nodeTypes.Select(n => n.Id).ToHashSet();

        var duplicateEdgeIds = edgeTypes.GroupBy(e => e.Id).Where(g => g.Count() > 1).ToList();
        foreach (var group in duplicateEdgeIds)
            errors.Add($"Duplicate EdgeType Id '{group.Key}' in: {string.Join(", ", group.Select(x => x.DescriptorType.Name))}");

        var duplicateEdgeKeys = edgeTypes.GroupBy(e => e.Key, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).ToList();
        foreach (var group in duplicateEdgeKeys)
            errors.Add($"Duplicate EdgeType Key '{group.Key}' in: {string.Join(", ", group.Select(x => x.DescriptorType.Name))}");

        foreach (var edgeType in edgeTypes)
        {
            if (!nodeTypeIds.Contains(edgeType.SourceNodeTypeId))
                errors.Add($"EdgeType '{edgeType.Key}' source NodeTypeId '{edgeType.SourceNodeTypeId}' is not registered.");

            if (!nodeTypeIds.Contains(edgeType.TargetNodeTypeId))
                errors.Add($"EdgeType '{edgeType.Key}' target NodeTypeId '{edgeType.TargetNodeTypeId}' is not registered.");
        }
    }
}
