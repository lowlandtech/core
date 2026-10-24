namespace LowlandTech.Core.WorkerProtocol;

/// <summary>
/// Snapshot of a Coder worker's per-workspace state. Returned by
/// <see cref="ICoderWorker.GetStateAsync"/> and broadcast to the
/// session's SignalR group on every state change. The frontend's
/// <c>CoderRealtimeCoordinator</c> consumes this to decide whether a
/// block needs to subscribe to live events or just hydrate from
/// <c>GET /events?since=</c>.
/// </summary>
public sealed class CoderStateSnapshot
{
    /// <summary>Worker id (one worker per active workspace+branch).</summary>
    public required Guid WorkerId { get; init; }

    public required Guid WorkspaceId { get; init; }
    public required string Branch { get; init; }
    public required Guid SessionId { get; init; }

    /// <summary>
    /// One of: "idle" (no prompt in flight), "running" (CLI process up),
    /// "awaiting_approval" (a Propose-branch / MR step needs the user),
    /// "cancelling" (Cancel was called, draining), "cancelled",
    /// "completed", "failed", "timed_out".
    /// </summary>
    public required string Status { get; init; }

    /// <summary>Prompt currently being processed, or null if idle.</summary>
    public Guid? CurrentPromptId { get; init; }

    /// <summary>CLI process id (the OS-level pid), when running.</summary>
    public int? ProcessId { get; init; }

    /// <summary>
    /// Wall-clock seconds since the current run started. Null when idle.
    /// </summary>
    public double? ElapsedSeconds { get; init; }

    /// <summary>Most recent event sequence number; null when idle.</summary>
    public long? LastEventSequence { get; init; }

    /// <summary>Server-side UTC timestamp of the snapshot.</summary>
    public required DateTimeOffset CapturedAt { get; init; }
}
