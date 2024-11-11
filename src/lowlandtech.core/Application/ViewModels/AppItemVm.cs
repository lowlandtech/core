namespace LowlandTech.Core.Application.ViewModels;

/// <summary>
/// Represents an application item view model.
/// </summary>
public class AppItemVm : TreeItemData<string>
{
    /// <summary>
    /// Creates a new instance of the <see cref="AppItemVm"/> class.
    /// </summary>
    /// <param name="id">The entity id.</param>
    /// <param name="name">The entity name.</param>
    /// <param name="icon">The entity icon.</param>
    /// <param name="entityId">The entity type id.</param>
    /// <param name="parent">The entity parent</param>
    public AppItemVm(Guid id, string? name, string? icon, Guid entityId, AppItemVm? parent) : base(name)
    {
        Id = id;
        Text = name;
        Icon = icon;
        EntityId = entityId;
        Parent = parent;
        Children = [];
    }

    /// <summary>
    /// Gets/sets the entity id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the entity icon.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets/sets the entity path.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets/sets is the is expanded flag.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets/sets is the is selected flag.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets/sets the entity is header flag.
    /// </summary>
    public bool IsHeader { get; set; }

    /// <summary>
    /// Gets/sets the entity parent.
    /// </summary>
    public AppItemVm? Parent { get; set; }
}