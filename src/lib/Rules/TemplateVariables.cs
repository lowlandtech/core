using System.Diagnostics.CodeAnalysis;

namespace LowlandTech.Core.Rules;

public static class TemplateVariables
{
    [RequiresUnreferencedCode(
        "Parsing template variables deserializes an untyped JSON document, so the value types cannot be statically analyzed.")]
    public static IReadOnlyDictionary<string, object?> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, object?>();
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)
               ?? new Dictionary<string, object?>();
    }

    [RequiresUnreferencedCode(
        "Parsing template rules deserializes a JSON document, so the rule expressions cannot be statically analyzed.")]
    public static IReadOnlyList<TemplateRule> ParseRules(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];
        return JsonSerializer.Deserialize<string[]>(json)!
               .Select(e => new TemplateRule(e))
               .ToList();
    }
}
