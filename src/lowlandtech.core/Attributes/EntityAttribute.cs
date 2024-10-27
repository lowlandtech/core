namespace LowlandTech.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EntityAttribute(string id) : Attribute
{
    public Guid EntityId { get; } = new(id);
}
