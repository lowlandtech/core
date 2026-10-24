
namespace LowlandTech.Core.Abstractions;

/// <summary>
/// Abstraction for the host agent service that delegates A2A calls to an <see cref="IA2AClient"/>.
/// Implemented by the core app and consumable by plugin frontends.
/// </summary>
public interface IHostAgentService
{
    bool IsConnected { get; }
    AgentCard? AgentCard { get; }

    Task<bool> ConnectAsync(CancellationToken ct = default);

    Task<HostAgentResponse> SendAsync(
        string userMessage,
        HostAgentOnboardContext context,
        CancellationToken ct = default);

    Task<HostAgentResponse> SendSessionMessageAsync(
        string userMessage,
        string entityType,
        Guid? entityId,
        List<Dictionary<string, string>>? sessionContext = null,
        List<HostAgentConversationTurn>? history = null,
        CancellationToken ct = default);

    /// <summary>
    /// Stream the agent response token-by-token. Each yielded string is an incremental
    /// text delta (not the full accumulated text). Completes when the agent finishes.
    /// </summary>
    IAsyncEnumerable<string> SendSessionMessageStreamAsync(
        string userMessage,
        string entityType,
        Guid? entityId,
        List<Dictionary<string, string>>? sessionContext = null,
        List<HostAgentConversationTurn>? history = null,
        CancellationToken ct = default);
}

public record HostAgentConversationTurn(string Role, string Content);

public record HostAgentOnboardContext
{
    public string Step { get; init; } = "welcome";
    public string? UserName { get; init; }
    public string? Intent { get; init; }
    public bool IsAuthenticated { get; init; }
}

public class HostAgentResponse
{
    public bool UseFallback { get; set; }
    public List<string> Messages { get; set; } = [];
    public List<(string Label, string Value)> Choices { get; set; } = [];
    public List<HostAgentBlockDescriptor> Blocks { get; set; } = [];
    public bool ExpectsInput { get; set; }
    public string? InputPlaceholder { get; set; }
    public string? NavigateTo { get; set; }
    public string? NextStep { get; set; }

    public static HostAgentResponse Fallback() => new() { UseFallback = true };
}

public record HostAgentBlockDescriptor(string BlockType, string BlockData);
