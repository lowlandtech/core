namespace LowlandTech.Core.Extensions;

public static class NavigationManagerExtensions
{
    /// <summary>
    /// Determines if the current URI is different from the base URI.
    /// </summary>
    /// <param name="navigationManager">The NavigationManager instance</param>
    /// <returns>True if the current URI is not the base URI</returns>
    public static bool IsNotBaseUri(this NavigationManager navigationManager)
    {
        return navigationManager.Uri != navigationManager.BaseUri;
    }

    public static bool IsBaseUri(this NavigationManager navigationManager)
    {
        return navigationManager.Uri == navigationManager.BaseUri;
    }
}