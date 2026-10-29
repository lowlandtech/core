namespace LowlandTech.Core.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class GivenAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
