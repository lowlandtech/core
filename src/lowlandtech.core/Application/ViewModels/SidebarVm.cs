namespace LowlandTech.Core.Application.ViewModels;

/// <summary>
/// Represents a sidebar view model.
/// </summary>
/// <param name="title">The sidebar name.</param>
/// <param name="items">The sidebar items.</param>
public class SidebarVm(string title, List<SidebarItem> items) : ObservableObject
{
    private string _title = title;
    /// <summary>
    /// Gets/sets the sidebar title.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private List<SidebarItem> _items = items;
    /// <summary>
    /// Gets/sets the sidebar items.
    /// </summary>
    public List<SidebarItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
}