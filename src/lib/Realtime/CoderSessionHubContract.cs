namespace LowlandTech.Core.Realtime;

/// <summary>
/// Constants shared between the Coder SignalR hub on the API side
/// (<c>LowlandTech.Api.Hubs.CoderSessionHub</c>) and the realtime
/// client on the frontend side
/// (<c>LowlandTech.Shell.Features.Coder.Realtime.CoderRealtimeClient</c>).
/// Lives in <c>LowlandTech.Core</c> so both ends agree on the wire
/// contract without crossing the API ↔ Shell reference direction.
/// </summary>
public static class CoderSessionHubContract
{
    /// <summary>Path the hub is mapped at, relative to the API base URL.</summary>
    public const string HubPath = "/hubs/coder";

    /// <summary>
    /// Name of the SignalR group that carries events for one session.
    /// Frontend joins on <c>JoinSession(sessionId)</c>; server-side
    /// publishers call <c>Clients.Group(GroupName(sessionId))</c>.
    /// </summary>
    public static string GroupName(Guid sessionId) => $"coder:{sessionId:N}";

    /// <summary>
    /// Server-to-client event emitted whenever a Coder event lands in
    /// the worker's per-workspace SQLite and the publisher broadcasts
    /// it. Payload: the <see cref="WorkerProtocol.CoderEvent"/>
    /// record (serialised via the default SignalR JSON contract).
    /// </summary>
    public const string CoderEventReceived = "coderEventReceived";
}
