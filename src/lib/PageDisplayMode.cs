namespace LowlandTech.Core;

/// <summary>
/// Layout variant for a block-style CRUD page. Drives the
/// <c>data-page-mode</c> attribute the Playwright page-contract tests
/// read, and (via <see cref="PageDisplayModeExtensions.ToLayoutClass(PageDisplayMode)"/>)
/// the CSS-Grid class on the page root.
/// <list type="bullet">
/// <item><see cref="PageOnly"/> — grid fills; selection updates state but no form rendered.</item>
/// <item><see cref="FormOnly"/> — form fills; no grid, no pagination.</item>
/// <item><see cref="SplitFormLeft"/> — form on left, grid on right.</item>
/// <item><see cref="SplitFormRight"/> — grid on left, form on right.</item>
/// <item><see cref="SplitFormTop"/> — form stacked above grid.</item>
/// <item><see cref="SplitFormBottom"/> — grid above form.</item>
/// </list>
/// </summary>
public enum PageDisplayMode
{
    PageOnly,
    FormOnly,
    SplitFormLeft,
    SplitFormRight,
    SplitFormTop,
    SplitFormBottom,
}

/// <summary>CSS class + data-attribute helpers for <see cref="PageDisplayMode"/>.
/// Kept as extension methods (not a field on the enum) so the conversion is a
/// zero-allocation switch expression callable from any page without DI.</summary>
public static class PageDisplayModeExtensions
{
    /// <summary>Returns the CSS-Grid layout class applied to the page root.
    /// Pair with the matching rule in the page's isolated <c>.razor.css</c> —
    /// each block page's companion stylesheet declares these six classes.</summary>
    public static string ToLayoutClass(this PageDisplayMode mode) => mode switch
    {
        PageDisplayMode.FormOnly         => "form-only",
        PageDisplayMode.SplitFormLeft    => "split split-form-left",
        PageDisplayMode.SplitFormRight   => "split split-form-right",
        PageDisplayMode.SplitFormTop     => "split split-form-top",
        PageDisplayMode.SplitFormBottom  => "split split-form-bottom",
        _ => "page-only",
    };

    /// <summary>Lowercase invariant string for the <c>data-page-mode</c>
    /// attribute. Matches the Playwright contract tests' expected values
    /// (pageonly / formonly / splitformleft / ...).</summary>
    public static string ToDataAttribute(this PageDisplayMode mode)
        => mode.ToString().ToLowerInvariant();
}
