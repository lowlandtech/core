
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks background job registrations contributed by features and their start/stop state.
/// SPEC2050 US10.
/// </summary>
public interface IJobRegistry
{
    IReadOnlyCollection<BackgroundJobDescriptor> All { get; }

    /// <summary>Job ids currently in the running state.</summary>
    IReadOnlyCollection<string> Running { get; }

    /// <exception cref="DuplicatePluginIdException">A job with the same id is already registered.</exception>
    void Register(BackgroundJobDescriptor descriptor);

    /// <summary>Remove a job (called on feature disable / uninstall). Stops it first if running.</summary>
    bool Unregister(string jobId);

    /// <summary>Remove every job belonging to a feature (called on feature disable / uninstall).</summary>
    void RemoveByFeature(string featureId);

    /// <summary>Mark a job as started.</summary>
    void Start(string jobId);

    /// <summary>Mark a job as stopped.</summary>
    void Stop(string jobId);

    bool IsRunning(string jobId);
}

public sealed class JobRegistry : DescriptorRegistry<string, BackgroundJobDescriptor>, IJobRegistry
{
    private readonly ConcurrentDictionary<string, byte> _running = new(StringComparer.OrdinalIgnoreCase);

    public JobRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(BackgroundJobDescriptor descriptor) => descriptor.Id;

    protected override void OnDuplicate(string key, BackgroundJobDescriptor existing, BackgroundJobDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    public IReadOnlyCollection<string> Running => _running.Keys.ToArray();

    public override bool Unregister(string jobId)
    {
        _running.TryRemove(jobId, out _);
        return base.Unregister(jobId);
    }

    public void RemoveByFeature(string featureId)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        RemoveWhere(d =>
        {
            if (!string.Equals(d.FeatureId, featureId, StringComparison.OrdinalIgnoreCase))
                return false;
            _running.TryRemove(d.Id, out _);
            return true;
        });
    }

    public void Start(string jobId)
    {
        ArgumentNullException.ThrowIfNull(jobId);
        if (!IsRegistered(jobId))
            throw new InvalidOperationException($"Job '{jobId}' is not registered.");
        _running[jobId] = 0;
    }

    public void Stop(string jobId)
    {
        ArgumentNullException.ThrowIfNull(jobId);
        _running.TryRemove(jobId, out _);
    }

    public bool IsRunning(string jobId)
    {
        ArgumentNullException.ThrowIfNull(jobId);
        return _running.ContainsKey(jobId);
    }
}
