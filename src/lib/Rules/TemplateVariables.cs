namespace LowlandTech.Core.Rules;

public static class TemplateVariables
{
    public static IReadOnlyDictionary<string, object?> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, object?>();
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)
               ?? new Dictionary<string, object?>();
    }

    public static IReadOnlyList<TemplateRule> ParseRules(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];
        return JsonSerializer.Deserialize<string[]>(json)!
               .Select(e => new TemplateRule(e))
               .ToList();
    }
}
