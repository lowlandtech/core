namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Contract for an entity.
/// </summary>
public interface IEntity { }

/// <summary>
/// Contract for an entity with an identifier.
/// </summary>
/// <typeparam name="TId"></typeparam>
public interface IEntity<TId> : IEntity
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    TId Id { get; set; }
}
