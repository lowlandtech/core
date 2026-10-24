namespace LowlandTech.Core;

/// <summary>
/// Service for managing application theme (light/dark mode).
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets whether the dark theme is currently active.
    /// </summary>
    bool IsDark { get; }

    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    event Action? ThemeChanged;

    /// <summary>
    /// Initialize the theme service (load saved preference).
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Toggle between light and dark themes.
    /// </summary>
    Task ToggleThemeAsync();

    /// <summary>
    /// Set the theme explicitly.
    /// </summary>
    /// <param name="isDark">True for dark theme, false for light.</param>
    Task SetThemeAsync(bool isDark);
}
