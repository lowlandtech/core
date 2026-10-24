namespace LowlandTech.Core.WorkerProtocol;

/// <summary>
/// Configuration for <see cref="ICoderWorker"/> implementations. Read once
/// at API startup and passed to whichever <c>ICoderWorker</c> impl DI
/// resolves. The in-process worker (Phase 1) and the container worker
/// (Phase 2) both consume the same shape so the swap is a single DI line.
/// </summary>
public sealed class CoderWorkerOptions
{
    /// <summary>
    /// Identifier of the resolved impl. "LocalProcess" for Phase 1
    /// (in-process, spawns the CLI via <c>IProcessRunner</c>); a URL for
    /// Phase 2 (container worker base URL).
    /// </summary>
    public string Mode { get; set; } = "LocalProcess";

    /// <summary>
    /// Base URL of the remote container worker. Only honoured when
    /// <see cref="Mode"/> is "Container". Phase 1 leaves this empty.
    /// </summary>
    public string? RemoteEndpoint { get; set; }

    /// <summary>
    /// Bearer token sent in the worker-token header on every call to
    /// <see cref="RemoteEndpoint"/>. Phase 2 only; Phase 1 ignores it.
    /// </summary>
    public string? WorkerToken { get; set; }

    /// <summary>
    /// Path to the worker's per-workspace SQLite. Defaults to
    /// <c>{Workspaces:Root}/{workspaceId}/{branch}/coder.db</c> via
    /// <c>WorkspaceSettings.GetWorktreePath(workspaceId, branch)</c>.
    /// Consumers that need a different layout (e.g. tests pointing at a
    /// <c>:memory:</c> connection) can override.
    /// </summary>
    public string? DatabasePathOverride { get; set; }

    /// <summary>
    /// Default CLI runtime key for new sessions. One of "claude", "codex",
    /// "kimi" — must match a key registered in
    /// <c>CliAgentRuntimeMap</c>. The user can override per-session via
    /// the Coder mode picker.
    /// </summary>
    public string DefaultRuntime { get; set; } = "claude";

    /// <summary>
    /// Default Coder model key (e.g. "claude-sonnet-4.6", "k2p6", "gpt-5.4").
    /// Matched against <c>CliAgentRuntimeMap.GetCoderModels</c>.
    /// </summary>
    public string DefaultModel { get; set; } = "claude-sonnet-4.6";

    /// <summary>
    /// Maximum wall-clock seconds a single worker run may take before
    /// <see cref="ICoderWorker.CancelAsync"/> is called and the run is
    /// marked as <c>timed_out</c>. Default 30 minutes.
    /// </summary>
    public TimeSpan RunTimeout { get; set; } = TimeSpan.FromMinutes(30);
}
