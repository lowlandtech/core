namespace LowlandTech.Core.Tests;

[AttributeUsage(AttributeTargets.Class)]
public sealed class UserStoryAttribute(string id, string description) : Attribute
{
    public string Id { get; } = id;

    public string Description { get; } = description;
}
