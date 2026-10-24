
namespace LowlandTech.Core.A2A;

// ── Agent Card (/.well-known/agent.json) ──

public class AgentCard
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? Version { get; set; }
    public AgentCapabilities Capabilities { get; set; } = new();
    public List<AgentSkill> Skills { get; set; } = [];
}

public class AgentCapabilities
{
    public bool Streaming { get; set; }
    public bool PushNotifications { get; set; }
}

public class AgentSkill
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = [];
}

// ── JSON-RPC envelope ──

public class JsonRpcRequest
{
    public string Jsonrpc { get; set; } = "2.0";
    public string Method { get; set; } = string.Empty;
    public string? Id { get; set; }
    public object? Params { get; set; }
}

public class JsonRpcResponse<T>
{
    public string Jsonrpc { get; set; } = "2.0";
    public string? Id { get; set; }
    public T? Result { get; set; }
    public JsonRpcError? Error { get; set; }
}

public class JsonRpcError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

// ── Task ──

public class A2ATask
{
    public string Id { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public A2ATaskStatus Status { get; set; } = new();
    public List<A2AMessage> History { get; set; } = [];
    public List<A2AArtifact> Artifacts { get; set; } = [];
    public Dictionary<string, object>? Metadata { get; set; }
}

public class A2ATaskStatus
{
    public A2ATaskState State { get; set; } = A2ATaskState.Submitted;
    public A2AMessage? Message { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum A2ATaskState
{
    Submitted,
    Working,
    InputRequired,
    Completed,
    Canceled,
    Failed
}

// ── Message & Parts ──

public class A2AMessage
{
    public A2ARole Role { get; set; }
    public List<A2APart> Parts { get; set; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum A2ARole
{
    User,
    Agent
}

[JsonDerivedType(typeof(TextPart), "text")]
[JsonDerivedType(typeof(DataPart), "data")]
public abstract class A2APart
{
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TextPart : A2APart
{
    public string Text { get; set; } = string.Empty;
}

public class DataPart : A2APart
{
    public Dictionary<string, object> Data { get; set; } = [];
}

// ── Artifact ──

public class A2AArtifact
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<A2APart> Parts { get; set; } = [];
}

// ── Request params ──

public class SendTaskParams
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? SessionId { get; set; }
    public A2AMessage Message { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class GetTaskParams
{
    public string Id { get; set; } = string.Empty;
}

public class CancelTaskParams
{
    public string Id { get; set; } = string.Empty;
}
