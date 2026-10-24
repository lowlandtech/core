namespace LowlandTech.Core;

public interface ITemplateExecutor
{
    string Render(string template, string json);
    TemplateValidationResult Validate(string template, string json, string expectedText);

    /// <summary>
    /// Renders a template version with variables and rules evaluation.
    /// Returns null if any rule evaluates to false (caller should skip).
    /// </summary>
    string? Render(Domain.Entities.TemplateVersion version, IReadOnlyDictionary<string, object?> context);
}

public record TemplateValidationResult(bool Valid, string Output, string Expected);
