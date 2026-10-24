namespace LowlandTech.Core.WorkerProtocol;

/// <summary>
/// Publishes one Coder event to both the worker's per-workspace SQLite
/// (durable log) and the session's SignalR group (live tail). The two
/// are atomic: the DB write happens first, and only on success does
/// the broadcast fire. A DB write failure means the broadcast does
/// not happen — the cold-path tail fetch from disk picks it up later.
/// </summary>
/// <remarks>
/// The contract is async and fire-and-forget on the broadcast side;
/// the return type is the broadcast result so the worker can log
/// failures without blocking on the SignalR round trip.
/// </remarks>
public interface ICoderEventPublisher
{
    /// <summary>
    /// Append the event to the per-workspace SQLite and broadcast it
    /// to the session's SignalR group. Sequence is the publisher's
    /// responsibility — it allocates the next monotonic value for the
    /// worker and stamps it on the event before either write.
    /// </summary>
    /// <returns>
    /// The event as written, with its assigned <c>Sequence</c>. If the
    /// DB write failed, returns null and does not broadcast.
    /// </returns>
    Task<CoderEvent?> PublishAsync(CoderEvent ev, CancellationToken ct = default);
}
