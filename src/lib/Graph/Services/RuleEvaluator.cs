
namespace LowlandTech.Core.Graph.Services;

/// <summary>
/// Evaluates <see cref="Rule"/> predicates (Left Operator Right) and
/// <see cref="RuleGroup"/> trees (and/or/not) against a flat bag of
/// <see cref="Property"/> values.
/// </summary>
public partial class RuleEvaluator : IRuleEvaluator
{
    // Matches numeric literals so we can do numeric comparison when possible
    [GeneratedRegex(@"^-?\d+(\.\d+)?$")]
    private static partial Regex NumericRegex();

    // ------------------------------------------------------------------ //
    //  IRuleEvaluator                                                      //
    // ------------------------------------------------------------------ //

    /// <inheritdoc />
    public RuleResult Evaluate(IEnumerable<Property> properties)
    {
        var props = properties.ToList();

        // Collect every rule reachable from the supplied properties
        var rules = props
            .SelectMany(p => p.Rules)
            .Distinct()
            .ToList();

        var violations = new List<RuleViolation>();

        foreach (var rule in rules)
        {
            if (!EvaluateRule(rule, props))
            {
                var propKey = rule.Property?.Key ?? rule.PropertyId?.ToString() ?? string.Empty;
                violations.Add(new RuleViolation(
                    rule.Id,
                    propKey,
                    rule.Message ?? $"Rule {rule.Id} failed."));
            }
        }

        return new RuleResult(violations.Count == 0, violations);
    }

    /// <inheritdoc />
    public RuleResult Evaluate(RuleGroup group, IEnumerable<Property> properties)
    {
        var props = properties.ToList();
        var passed = EvaluateGroup(group, props);

        if (passed)
            return new RuleResult(true, []);

        // Collect leaf-rule violations for reporting
        var violations = CollectGroupViolations(group, props);
        return new RuleResult(false, violations);
    }

    // ------------------------------------------------------------------ //
    //  Core evaluation                                                     //
    // ------------------------------------------------------------------ //

    private bool EvaluateGroup(RuleGroup group, IReadOnlyList<Property> properties)
    {
        var op = group.Operator?.ToLowerInvariant() ?? "and";

        // Leaf rules inside this group
        var ruleResults = group.Rules.Select(r => EvaluateRule(r, properties)).ToList();

        // Results from child groups
        var childResults = group.Children.Select(c => EvaluateGroup(c, properties)).ToList();

        var all = ruleResults.Concat(childResults).ToList();

        if (all.Count == 0) return true;

        return op switch
        {
            "or"  => all.Any(r => r),
            "not" => !all.All(r => r),
            _     => all.All(r => r),   // "and" (default)
        };
    }

    private static bool EvaluateRule(Rule rule, IReadOnlyList<Property> properties)
    {
        // Resolve left operand: treat it as a property key first, then as a literal
        var left = ResolveOperand(rule.Left, properties);

        var op = rule.Operator?.ToLowerInvariant() ?? "==";

        // Unary operators
        if (op is "isnull")   return left is null;
        if (op is "isnotnull") return left is not null;

        var right = ResolveOperand(rule.Right, properties);

        return op switch
        {
            "=="          => CompareValues(left, right) == 0,
            "!="          => CompareValues(left, right) != 0,
            ">"           => CompareValues(left, right) > 0,
            ">="          => CompareValues(left, right) >= 0,
            "<"           => CompareValues(left, right) < 0,
            "<="          => CompareValues(left, right) <= 0,
            "contains"    => Contains(left, right),
            "startswith"  => StartsWith(left, right),
            "endswith"    => EndsWith(left, right),
            "matches"     => Matches(left, right),
            "in"          => InList(left, right),
            "notin"       => !InList(left, right),
            _             => CompareValues(left, right) == 0,
        };
    }

    // ------------------------------------------------------------------ //
    //  Operand resolution                                                  //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// If <paramref name="operand"/> matches a property key in the bag, return
    /// that property's value; otherwise treat the operand itself as the literal.
    /// </summary>
    private static string? ResolveOperand(string? operand, IReadOnlyList<Property> properties)
    {
        if (operand is null) return null;

        var match = properties.FirstOrDefault(p =>
            string.Equals(p.Key, operand, StringComparison.OrdinalIgnoreCase));

        return match is not null ? match.Value : operand;
    }

    // ------------------------------------------------------------------ //
    //  Comparison helpers                                                  //
    // ------------------------------------------------------------------ //

    private static int CompareValues(string? left, string? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return -1;
        if (right is null) return 1;

        // Numeric comparison when both sides look like numbers
        if (NumericRegex().IsMatch(left) && NumericRegex().IsMatch(right) &&
            double.TryParse(left,  System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var l) &&
            double.TryParse(right, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var r))
        {
            return l.CompareTo(r);
        }

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static bool Contains(string? left, string? right)
        => left is not null && right is not null &&
           left.Contains(right, StringComparison.OrdinalIgnoreCase);

    private static bool StartsWith(string? left, string? right)
        => left is not null && right is not null &&
           left.StartsWith(right, StringComparison.OrdinalIgnoreCase);

    private static bool EndsWith(string? left, string? right)
        => left is not null && right is not null &&
           left.EndsWith(right, StringComparison.OrdinalIgnoreCase);

    private static bool Matches(string? left, string? right)
    {
        if (left is null || right is null) return false;
        try { return Regex.IsMatch(left, right); }
        catch { return false; }
    }

    /// <summary>Right operand is a comma-separated list of values.</summary>
    private static bool InList(string? left, string? right)
    {
        if (left is null || right is null) return false;
        var items = right.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return items.Any(i => string.Equals(i, left, StringComparison.OrdinalIgnoreCase));
    }

    // ------------------------------------------------------------------ //
    //  Violation collection for RuleGroup overload                        //
    // ------------------------------------------------------------------ //

    private static List<RuleViolation> CollectGroupViolations(
        RuleGroup group,
        IReadOnlyList<Property> properties)
    {
        var violations = new List<RuleViolation>();

        foreach (var rule in group.Rules)
        {
            if (!EvaluateRule(rule, properties))
            {
                var propKey = rule.Property?.Key ?? rule.PropertyId?.ToString() ?? string.Empty;
                violations.Add(new RuleViolation(
                    rule.Id,
                    propKey,
                    rule.Message ?? $"Rule {rule.Id} failed."));
            }
        }

        foreach (var child in group.Children)
            violations.AddRange(CollectGroupViolations(child, properties));

        return violations;
    }
}
