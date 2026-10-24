namespace LowlandTech.Core.WorkerProtocol;

/// <summary>
/// The contract the API exposes to itself (Phase 1, in-process) and
/// to the container worker (Phase 2, remote HTTP). The in-process
/// implementation writes directly to the per-workspace SQLite; the
/// container implementation forwards to a remote HTTP service that
/// returns rows from the worker's own SQLite. From the API's
/// perspective, the two are interchangeable.
/// </summary>
/// <remarks>
/// All methods are idempotent under network retry: a duplicate
/// <see cref="DispatchAsync"/> with the same <c>promptId</c> resolves
/// to the existing run, not a new one. This is the contract the
/// client-side outbox relies on for at-least-once delivery.
/// </remarks>
public interface ICoderWorker
{
    /// <summary>
    /// Submit a prompt. Resolves immediately with the worker id (one
    /// worker per active workspace+branch). The events that follow
    /// are delivered through the SignalR group and the per-workspace
    /// SQLite — not through this call's return value.
    /// </summary>
    /// <returns>
    /// The id of the worker that owns the run. The frontend stores
    /// this on its <c>CoderStateSnapshot</c> for cross-tab routing.
    /// </returns>
    Task<Guid> DispatchAsync(CoderPrompt prompt, CancellationToken ct = default);

    /// <summary>
    /// Request cancellation of a running prompt. Idempotent: calling
    /// on an already-cancelled or already-completed worker is a no-op
    /// that returns the final <see cref="CoderStateSnapshot"/>.
    /// </summary>
    Task<CoderStateSnapshot> CancelAsync(Guid workerId, CancellationToken ct = default);

    /// <summary>
    /// Read the current state of a worker. Cold-path call: the
    /// frontend uses it on tab focus / block mount to hydrate the
    /// block from disk before subscribing to the SignalR stream.
    /// </summary>
    Task<CoderStateSnapshot?> GetStateAsync(Guid workerId, CancellationToken ct = default);

    /// <summary>
    /// Tail events from the worker's per-workspace SQLite starting
    /// from <paramref name="sinceSequence"/> (exclusive). Used by the
    /// frontend's <c>CoderEventFetcher</c> to hydrate cold sessions.
    /// Returns up to <paramref name="limit"/> events ordered by
    /// sequence ascending.
    /// </summary>
    Task<IReadOnlyList<CoderEvent>> GetEventsAsync(
        Guid    workerId,
        long    sinceSequence,
        int     limit          = 200,
        CancellationToken ct   = default);

    /// <summary>
    /// Resolve the worker id for a (workspace, branch) pair, creating
    /// one on first call. Idempotent: a second call with the same
    /// pair returns the existing worker id.
    /// </summary>
    Task<Guid> EnsureWorkerAsync(
        Guid    workspaceId,
        string  branch,
        CancellationToken ct = default);

    /// <summary>
    /// Drop the worker for a (workspace, branch) and delete its
    /// per-workspace SQLite. Called when the workspace closes
    /// (per the roadmap: "deleted when the workspace closes").
    /// </summary>
    Task CloseAsync(Guid workerId, CancellationToken ct = default);
}
