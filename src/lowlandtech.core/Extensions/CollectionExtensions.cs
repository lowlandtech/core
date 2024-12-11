namespace LowlandTech.Core.Extensions;

/// <summary>
/// Represents a set of extension methods for <see cref="ICollection{T}"/> objects.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Add range of items to the collection.
    /// </summary>
    /// <typeparam name="T">Is the item.</typeparam>
    /// <param name="destination">Is the destination list.</param>
    /// <param name="source">is the source list.</param>
    public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
    {
        if (destination is List<T> list)
        {
            list.AddRange(source);
        }
        else
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }
    }
}