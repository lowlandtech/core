namespace LowlandTech.Core.Rules;

public record TemplateRule(string Expression)
{
    public bool Evaluate(IReadOnlyDictionary<string, object?> context)
    {
        var expr = new Expression(Expression);
        foreach (var (k, v) in context)
            expr.Parameters[k] = v;
        return expr.Evaluate() is true;
    }
}
