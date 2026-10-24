namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Describes a read-shaping policy for an entity type.
/// Used by GraphSession to apply includes/projections without hardcoded branching.
/// </summary>
public interface IGraphQueryPolicyDescriptor
{
    Type EntityType { get; }
    IQueryable Apply(IQueryable query);
}

/// <summary>
/// Resolves and applies query shaping policies for entity reads.
/// </summary>
public interface IGraphQueryPolicyRegistry
{
    IQueryable<T> ApplyReadPolicy<T>(IQueryable<T> query) where T : class;
}
