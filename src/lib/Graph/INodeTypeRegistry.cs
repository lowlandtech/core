
namespace LowlandTech.Core.Graph;

/// <summary>
/// Discovers INodeTypeDescriptor implementations and provides lookup.
/// Registered as a singleton in DI.
/// </summary>
public interface INodeTypeRegistry
{
    IReadOnlyList<NodeTypeEntry> All { get; }
    NodeTypeEntry? FindByKey(string key);
    NodeTypeEntry? FindById(Guid id);
    NodeTypeEntry? FindByEntityType(Type entityType);
}

public record NodeTypeEntry(
    Guid Id,
    string Key,
    string Label,
    string? Icon,
    string? BlockComponent,
    int SortOrder,
    Type? EntityType,
    Type DescriptorType,
    Func<object, Guid> ExtractId,
    Func<object, string> ExtractLabel,
    Func<object, EntityParentRef> ExtractParent,
    bool IsLeaf = false,
    bool IsHeader = false,
    bool IsListable = false,
    bool IsStatic = true,
    IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders = null,
    Func<Node, IReadOnlyList<Property>, object?>? Project = null);
