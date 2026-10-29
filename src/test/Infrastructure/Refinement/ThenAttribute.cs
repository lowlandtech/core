namespace LowlandTech.Core.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ThenAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
