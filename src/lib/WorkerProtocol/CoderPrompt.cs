namespace LowlandTech.Core.WorkerProtocol;

/// <summary>
/// A prompt the user submitted to a Coder session. Carries the workitem
/// the Coder is working on, the worktree it should operate in, and the
/// resolved CLI runtime + model so the worker doesn't have to re-resolve
/// what the user already chose in the mode picker.
/// </summary>
/// <param name="PromptId">
/// Client-generated id for the prompt. Used as the
/// <c>idempotencyKey</c> on the outbox row and echoed on every event the
/// worker emits in response, so the outbox drain can pair requests with
/// their replies.
/// </param>
/// <param name="SessionId">
/// The dashboard session this prompt belongs to. The Coder worker
/// uses it to scope the SignalR group that receives the events.
/// </param>
/// <param name="WorkitemId">
/// The workitem the Coder is operating on. The worker resolves this to
/// a workspace and worktree via the
/// <c>Workitem → Workspace</c> mapping (see roadmap Q1).
/// </param>
/// <param name="WorkspaceId">
/// The workspace the Coder should operate in. Resolved server-side from
/// the session's pinned <c>WorkspaceBlock</c> (or auto-provisioned via
/// <c>ICoderWorkspaceProvisioner</c> when missing).
/// </param>
/// <param name="Branch">
/// The branch (and worktree) the Coder should check out. Defaults to
/// <c>develop</c>; user-overridable per session.
/// </param>
/// <param name="RuntimeKey">
/// The CLI runtime the Coder should spawn: "claude", "codex", or "kimi".
/// Resolved from the user's mode picker; the worker uses
/// <c>CliAgentRuntimeMap.Resolve(runtimeKey)</c> to look up the
/// executable, args, and session-id regex.
/// </param>
/// <param name="ModelKey">
/// The Coder model the runtime should load. One of the keys returned by
/// <c>CliAgentRuntimeMap.GetCoderModels()</c>.
/// </param>
/// <param name="Text">The user-typed prompt body.</param>
public sealed record CoderPrompt(
    Guid                                  PromptId,
    Guid                                  SessionId,
    Guid                                  WorkitemId,
    Guid                                  WorkspaceId,
    string                                Branch,
    string                                RuntimeKey,
    string                                ModelKey,
    string                                Text);
