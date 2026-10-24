
namespace LowlandTech.Core.Graph;

public readonly record struct EntityParentRef(string? Key, Guid? Id)
{
    public bool HasValue => Key is not null && Id.HasValue;
    public static EntityParentRef None => default;
}

public interface INodeTypeDescriptor
{
    Guid Id { get; }
    string Key { get; }
    string Label { get; }
    string? Icon { get; }
    string? BlockComponent { get; }
    int SortOrder { get; }
    Type? EntityType { get; }

    virtual bool IsLeaf => false;

    /// <summary>
    /// Header NodeTypes group entities under a domain label in the user-context tree
    /// (e.g. "Workitems", "Workspaces"). They have no backing entity (<see cref="EntityType"/>
    /// is null) but they can declare outgoing EdgeTypes whose verb is "create" — those
    /// edges drive the dashboard's selection-bound Create-mode UX. Headers are not leaves
    /// in the structural sense, but they are leaves in the entity-creation sense (no
    /// children to navigate; you only create things under them).
    /// </summary>
    virtual bool IsHeader => false;

    /// <summary>
    /// Listable NodeTypes can be referenced as entries inside a <c>list</c> node
    /// via the <c>allows</c> relationship. Defaults to <c>false</c>.
    /// </summary>
    virtual bool IsListable => false;

    virtual IReadOnlyList<NavigationHeaderDefinition>? NavigationHeaders => null;

    virtual Guid ExtractId(object entity)
        => entity.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)?.GetValue(entity) is Guid id
            ? id
            : Guid.Empty;

    virtual string ExtractLabel(object entity)
        => entity.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)?.GetValue(entity) as string
            ?? string.Empty;

    virtual EntityParentRef ExtractParent(object entity) => EntityParentRef.None;
}
