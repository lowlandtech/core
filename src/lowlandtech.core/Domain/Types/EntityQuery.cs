namespace LowlandTech.Core.Domain.Types;

/// <summary>
/// Represents an entity query.
/// </summary>
public class EntityQuery
{
    /// <summary>
    /// Gets/sets the entity context identifier.
    /// </summary>
    public string DbContextId { get; set; } = null!;

    /// <summary>
    /// Gets/sets the entity dbset identifier.
    /// </summary>
    public string DbSetId { get; set; } = null!;

    /// <summary>
    /// Gets/sets the entity query conditions.
    /// </summary>
    public List<EntityCondition> Conditions { get; set; } = new();
}