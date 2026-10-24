
namespace LowlandTech.Core.Graph;

public record RuleResult(bool IsValid, IReadOnlyList<RuleViolation> Violations);
public record RuleViolation(Guid RuleId, string PropertyKey, string Message);

/// <summary>
/// Evaluates rules and rule groups against a set of properties.
/// Rules are expression predicates (Left Operator Right) that return true/false.
/// RuleGroups compose rules with boolean logic (and/or/not).
/// </summary>
public interface IRuleEvaluator
{
    /// <summary>
    /// Evaluate all rules associated with the given properties.
    /// Returns a result indicating whether all rules passed, with violation details.
    /// </summary>
    RuleResult Evaluate(IEnumerable<Property> properties);

    /// <summary>
    /// Evaluate a single rule group (and its nested children) against properties.
    /// </summary>
    RuleResult Evaluate(RuleGroup group, IEnumerable<Property> properties);
}
