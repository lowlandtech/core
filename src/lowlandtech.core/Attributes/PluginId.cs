namespace LowlandTech.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PluginId(string id) : Attribute
{
    public string Id { get; } = id;
}