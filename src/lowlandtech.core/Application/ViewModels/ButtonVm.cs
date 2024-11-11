namespace LowlandTech.Core.Application.ViewModels;

/// <summary>
/// Represents a button view model.
/// </summary>
/// <param name="text"></param>
/// <param name="icon"></param>
/// <param name="onClick"></param>
/// <param name="canClick"></param>
public class ButtonVm(string text, string? icon, Action onClick, Func<bool> canClick) : ObservableObject
{
    /// <summary>
    /// Gets/sets the button identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the button text.
    /// </summary>
    public string Text { get; set; } = text;

    /// <summary>
    /// Gets/sets the button icon.
    /// </summary>
    public string? Icon { get; set; } = icon;

    /// <summary>
    /// Gets/sets the button click action.
    /// </summary>
    public Action OnClick { get; set; } = onClick;

    /// <summary>
    /// Gets/sets the button can click function.
    /// </summary>
    public Func<bool> CanClick { get; } = canClick;

    /// <summary>
    /// Gets/sets the button alighment.
    /// </summary>
    public bool RightAligned { get; set; }

    /// <summary>
    /// Gets/sets the button type.
    /// </summary>
    public ButtonType ButtonType { get; set; } = ButtonType.Button;
}