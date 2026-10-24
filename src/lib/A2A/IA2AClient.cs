namespace LowlandTech.Core.A2A;

/// <summary>
/// Client for communicating with an A2A-compatible agent.
/// Swap implementations to test with different agent backends.
/// </summary>
public interface IA2AClient
{
    /// <summary>Fetch the agent's card (capabilities, skills).</summary>
    Task<AgentCard?> GetAgentCardAsync(CancellationToken ct = default);

    /// <summary>Send a message to the agent and get back the updated task.</summary>
    Task<A2ATask> SendTaskAsync(SendTaskParams request, CancellationToken ct = default);

    /// <summary>Retrieve an existing task by ID.</summary>
    Task<A2ATask?> GetTaskAsync(string taskId, CancellationToken ct = default);

    /// <summary>Cancel a running task.</summary>
    Task CancelTaskAsync(string taskId, CancellationToken ct = default);

    /// <summary>
    /// Send a task and stream intermediate status updates as they arrive via SSE.
    /// Each yielded string is the accumulated text content from the latest status message.
    /// Falls back gracefully if the agent does not support streaming.
    /// </summary>
    IAsyncEnumerable<string> SendTaskStreamAsync(SendTaskParams request, CancellationToken ct = default);
}
