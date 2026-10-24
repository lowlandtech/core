namespace LowlandTech.Core.Plugins;

/// <summary>
/// Cross-feature event router. Subscriptions are scoped to a feature so they can be
/// removed when the feature is disabled.
/// SPEC2050 US10.
/// </summary>
public interface IEventRouter
{
    IReadOnlyCollection<EventSubscriptionDescriptor> All { get; }

    /// <summary>Set of event types declared as publishable by features.</summary>
    IReadOnlyCollection<Type> PublishedEventTypes { get; }

    /// <summary>Declare an event type as publishable. Subsequent subscriptions for unknown types fail.</summary>
    void DeclareEvent(Type eventType);

    /// <summary>Register a subscription.</summary>
    /// <exception cref="UnknownEventTypeException">No publisher has declared this event type.</exception>
    void Subscribe(EventSubscriptionDescriptor descriptor);

    /// <summary>Remove every subscription owned by a feature (called on feature disable).</summary>
    void RemoveByFeature(string featureId);

    /// <summary>Synchronously dispatch an event to every registered handler. Returns the handler invocations performed.</summary>
    int Publish<TEvent>(TEvent payload, Func<Type, object> handlerFactory) where TEvent : class;
}

public sealed class EventRouter : IEventRouter
{
    private readonly List<EventSubscriptionDescriptor> _subscriptions = [];
    private readonly HashSet<Type> _publishedTypes = [];
    private readonly object _lock = new();

    public IReadOnlyCollection<EventSubscriptionDescriptor> All
    {
        get { lock (_lock) return _subscriptions.ToArray(); }
    }

    public IReadOnlyCollection<Type> PublishedEventTypes
    {
        get { lock (_lock) return _publishedTypes.ToArray(); }
    }

    public void DeclareEvent(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        lock (_lock) _publishedTypes.Add(eventType);
    }

    public void Subscribe(EventSubscriptionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        lock (_lock)
        {
            if (!_publishedTypes.Contains(descriptor.EventType))
                throw new UnknownEventTypeException(descriptor.EventType);
            _subscriptions.Add(descriptor);
        }
    }

    public void RemoveByFeature(string featureId)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        lock (_lock)
        {
            _subscriptions.RemoveAll(s => string.Equals(s.FeatureId, featureId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public int Publish<TEvent>(TEvent payload, Func<Type, object> handlerFactory) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(handlerFactory);

        EventSubscriptionDescriptor[] snapshot;
        lock (_lock)
        {
            snapshot = _subscriptions
                .Where(s => s.EventType == typeof(TEvent))
                .ToArray();
        }

        var invoked = 0;
        foreach (var subscription in snapshot)
        {
            var handler = handlerFactory(subscription.HandlerType);
            if (handler is IEventHandler<TEvent> typed)
            {
                typed.Handle(payload);
                invoked++;
            }
        }
        return invoked;
    }
}

/// <summary>Marker contract for cross-feature event handlers.</summary>
public interface IEventHandler<in TEvent> where TEvent : class
{
    void Handle(TEvent payload);
}
