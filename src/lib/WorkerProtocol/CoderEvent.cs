namespace LowlandTech.Core.WorkerProtocol;

/// <summary>
/// The discriminated union of events a Coder worker emits. Six derived
/// types cover the observable surface the Coder blocks and the outbox
/// drain need to react to. Every event is persisted to the worker's
/// per-workspace SQLite (via <c>CoderEventPublisher</c>) and broadcast
/// to the session's SignalR group atomically.
/// </summary>
/// <remarks>
/// Derived types are sealed records so equality is structural and the
/// <c>CoderEventKind</c> tag is the only switch the consumer needs.
/// New event types must be appended; existing ordinals must not change
/// because the per-workspace SQLite stores the kind as an integer.
/// </remarks>
[JsonDerivedType(typeof(WorkerStarted),    CoderEventKind.WorkerStartedValue)]
[JsonDerivedType(typeof(OutputChunk),      CoderEventKind.OutputChunkValue)]
[JsonDerivedType(typeof(ToolCall),         CoderEventKind.ToolCallValue)]
[JsonDerivedType(typeof(ToolResult),       CoderEventKind.ToolResultValue)]
[JsonDerivedType(typeof(ArtifactProduced), CoderEventKind.ArtifactProducedValue)]
[JsonDerivedType(typeof(WorkerStopped),    CoderEventKind.WorkerStoppedValue)]
public abstract record CoderEvent
{
    /// <summary>
    /// Monotonically-increasing sequence number scoped to a single
    /// worker's per-workspace SQLite. Assigned at publish time by
    /// <see cref="ICoderEventPublisher"/>; never reused. The frontend
    /// uses this for the <c>since=</c> tail-fetch in
    /// <c>GET /events?since={seq}</c>. The default of <c>0</c> is
    /// the sentinel for "unpublished" — events are immutable after
    /// publish, and the publisher stamps this with the next monotonic
    /// value via <c>with { Sequence = … }</c>.
    /// </summary>
    public long Sequence { get; init; }

    /// <summary>UTC timestamp the event was written.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>
    /// Prompt this event belongs to. Null only for events that aren't
    /// tied to a single prompt (e.g. <c>WorkerStarted</c> when the
    /// workspace first becomes active).
    /// </summary>
    public Guid? PromptId { get; init; }
}

/// <summary>The CLI process for a (workspace, branch) was spawned.</summary>
public sealed record WorkerStarted : CoderEvent
{
    public required string RuntimeKey { get; init; }
    public required string ModelKey { get; init; }
    public required int    ProcessId { get; init; }
    public required string Worktree  { get; init; }
}

/// <summary>A single chunk of stdout (or stderr, tagged) arrived.</summary>
public sealed record OutputChunk : CoderEvent
{
    /// <summary>"stdout" or "stderr".</summary>
    public required string Stream    { get; init; }
    public required string Text      { get; init; }
    /// <summary>True when the chunk completes a logical message boundary.</summary>
    public bool           IsBoundary { get; init; }
}

/// <summary>The CLI made a tool call (read, edit, bash, …).</summary>
public sealed record ToolCall : CoderEvent
{
    public required string ToolName   { get; init; }
    public required string Arguments  { get; init; }
    public string?        CallId      { get; init; }
}

/// <summary>
/// The CLI finished a tool call with this result. Always emitted
/// after the matching <see cref="ToolCall"/>; pairs on <c>CallId</c>.
/// </summary>
public sealed record ToolResult : CoderEvent
{
    public required string CallId    { get; init; }
    public required bool   Success   { get; init; }
    public string?         Output    { get; init; }
    public string?         ErrorText { get; init; }
}

/// <summary>
/// A checkpoint event: the Coder produced a diff, a branch, an MR
/// proposal, an index update, or a gap detection — anything the
/// frontend wants to render as a Coder block (file diff, branch
/// status, MR, RAG result, gap proposal, tool execution).
/// </summary>
public sealed record ArtifactProduced : CoderEvent
{
    /// <summary>One of: "file_diff", "branch_status", "merge_request", "rag_result", "gap_proposal", "tool_execution".</summary>
    public required string ArtifactType { get; init; }
    public required string PayloadJson  { get; init; }
}

/// <summary>
/// The CLI process exited. <c>ExitCode</c> matches the OS exit code;
/// <c>Status</c> is the worker's interpretation ("completed" / "failed"
/// / "cancelled" / "timed_out").
/// </summary>
public sealed record WorkerStopped : CoderEvent
{
    public required int    ExitCode { get; init; }
    public required string Status   { get; init; }
}

/// <summary>
/// Helper that returns the discriminant kind string for any
/// <see cref="CoderEvent"/> without forcing a switch on the consumer.
/// Mirrors the per-workspace SQLite <c>event_kind</c> column ordinals.
/// </summary>
public static class CoderEventKind
{
    public const string WorkerStartedValue    = "worker_started";
    public const string OutputChunkValue      = "output_chunk";
    public const string ToolCallValue         = "tool_call";
    public const string ToolResultValue       = "tool_result";
    public const string ArtifactProducedValue = "artifact_produced";
    public const string WorkerStoppedValue    = "worker_stopped";

    public static string Of(CoderEvent e) => e switch
    {
        WorkerStarted    => WorkerStartedValue,
        OutputChunk      => OutputChunkValue,
        ToolCall         => ToolCallValue,
        ToolResult       => ToolResultValue,
        ArtifactProduced => ArtifactProducedValue,
        WorkerStopped    => WorkerStoppedValue,
        _                => throw new ArgumentOutOfRangeException(nameof(e), e.GetType().Name, null)
    };
}
