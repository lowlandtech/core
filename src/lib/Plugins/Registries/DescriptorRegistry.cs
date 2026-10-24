
namespace LowlandTech.Core.Plugins.Registries;

/// <summary>
/// Thread-safe in-memory descriptor registry. Concrete registries derive from this
/// and supply <see cref="KeyOf"/>; per-registry duplicate semantics are customized by
/// overriding <see cref="OnDuplicate"/> (e.g. to throw a typed conflict exception).
/// SPEC2050 — template for US01, US03, US06, US09, US10, US11, US12, US13.
/// </summary>
public abstract class DescriptorRegistry<TKey, TDescriptor>
    where TKey : notnull
    where TDescriptor : class
{
    private readonly ConcurrentDictionary<TKey, TDescriptor> _store;

    protected DescriptorRegistry(IEqualityComparer<TKey>? comparer = null)
    {
        _store = comparer is null
            ? new ConcurrentDictionary<TKey, TDescriptor>()
            : new ConcurrentDictionary<TKey, TDescriptor>(comparer);
    }

    /// <summary>Extracts the storage key from a descriptor.</summary>
    protected abstract TKey KeyOf(TDescriptor descriptor);

    public IReadOnlyCollection<TDescriptor> All => _store.Values.ToArray();

    public bool IsRegistered(TKey key) => _store.ContainsKey(key);

    public TDescriptor? Find(TKey key) => _store.TryGetValue(key, out var d) ? d : null;

    public bool TryGet(TKey key, out TDescriptor descriptor)
    {
        if (_store.TryGetValue(key, out var d)) { descriptor = d; return true; }
        descriptor = null!;
        return false;
    }

    public virtual void Register(TDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        var key = KeyOf(descriptor);
        if (!_store.TryAdd(key, descriptor))
            OnDuplicate(key, _store[key], descriptor);
    }

    /// <summary>
    /// Invoked when <see cref="Register"/> is called with a key that already exists.
    /// Default behavior is to throw; override to throw a typed conflict exception.
    /// </summary>
    protected virtual void OnDuplicate(TKey key, TDescriptor existing, TDescriptor incoming)
        => throw new InvalidOperationException($"Descriptor with key '{key}' is already registered.");

    public virtual bool Unregister(TKey key) => _store.TryRemove(key, out _);

    /// <summary>
    /// Unconditionally replaces an existing entry in the store. Returns <c>true</c> if
    /// the key existed and was updated, <c>false</c> if the key was absent (no write).
    /// Intended for subclasses that need to update a descriptor in-place (e.g. SetEnabled,
    /// Remap) without triggering duplicate-check logic.
    /// </summary>
    protected bool Replace(TKey key, TDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!_store.ContainsKey(key)) return false;
        _store[key] = descriptor;
        return true;
    }

    /// <summary>
    /// Remove every descriptor matching <paramref name="predicate"/>. Used by
    /// subclasses to implement feature-scoped or plugin-scoped sweeps
    /// (<c>RemoveByFeature</c>, <c>RemoveByPlugin</c>) on top of the base store.
    /// Returns the number of descriptors removed.
    /// </summary>
    protected int RemoveWhere(Func<TDescriptor, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var removed = 0;
        foreach (var kvp in _store.ToArray())
        {
            if (predicate(kvp.Value) && _store.TryRemove(kvp.Key, out _))
                removed++;
        }
        return removed;
    }
}

