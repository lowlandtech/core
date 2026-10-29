namespace LowlandTech.Core.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class WhenAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
