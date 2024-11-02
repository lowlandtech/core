namespace LowlandTech.Core.Attributes;

/// <summary>
/// Sets the identifier for a context.
/// </summary>
/// <param name="id"></param>
[AttributeUsage(AttributeTargets.Class)]
public class DbContextIdAttribute(string id) : Attribute
{
    /// <summary>
    /// Gets the context identifier.
    /// </summary>
    public string Id => id;
}
