
namespace LowlandTech.Core.Graph;

public sealed record NavigationHeaderDefinition(
    string NavigationProperty,
    string HeaderLabel,
    string ChildNodeTypeKey,
    int SortOrder = 0,
    string? HeaderIcon = null,
    string? ParentToHeaderEdgeKey = null,
    string? HeaderToChildEdgeKey = null);

public static class NavigationHeaderConventions
{
    public static Guid EdgeTypeId(string key) => GuidV5.Create(GuidV5.Namespace, $"edgetype:{key}");

    public static string ParentToHeaderEdgeKey(string parentNodeTypeKey, string navigationProperty)
        => $"{parentNodeTypeKey}:contains:{NormalizeSegment(navigationProperty)}-header";

    public static string HeaderToChildEdgeKey(string childNodeTypeKey)
        => $"header:contains:{childNodeTypeKey}";

    private static string NormalizeSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasDash = false;

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
                previousWasDash = false;
                continue;
            }

            if (previousWasDash) continue;

            builder.Append('-');
            previousWasDash = true;
        }

        return builder.ToString().Trim('-');
    }
}