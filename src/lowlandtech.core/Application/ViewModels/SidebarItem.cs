namespace LowlandTech.Core.Application.ViewModels;

/// <summary>
/// Represents a sidebar item view model.
/// </summary>
/// <param name="text">The sidebar item text</param>
/// <param name="icon">The sidebar item icon.</param>
/// <param name="path">Teh sidebar item path.</param>
public class SidebarItem(string text, string icon, string path)
{
    /// <summary>
    /// Gets/sets the sidebar item text.
    /// </summary>
    public string Text { get; set; } = text;

    /// <summary>
    /// Gets/sets the sidebar item icon.
    /// </summary>
    public string Icon { get; set; } = icon;

    /// <summary>
    /// Gets/sets the sidebar item path.
    /// </summary>
    public string Path { get; set; } = path;
}