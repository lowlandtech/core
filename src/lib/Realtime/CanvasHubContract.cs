namespace LowlandTech.Core.Realtime;

/// <summary>
/// Constants shared between the SignalR hub on the API side
/// (<c>LowlandTech.Api.Hubs.CanvasHub</c>) and the realtime client on the
/// frontend side (<c>LowlandTech.Designer.Frontend.Services.CanvasRealtimeClient</c>).
/// Lives in <c>LowlandTech.Core</c> so both ends agree on the wire
/// contract without crossing the API ↔ Designer reference direction.
/// </summary>
public static class CanvasHubContract
{
    /// <summary>Path the hub is mapped at, relative to the API base URL.</summary>
    public const string HubPath = "/hubs/canvas";

    /// <summary>
    /// Server-to-client event emitted whenever a canvas mutates. Payload:
    /// the canvas's id as a string. Clients receive it and decide whether
    /// to refetch (active canvas → reload layout; either way → refresh
    /// the left list so created/deleted canvases land).
    /// </summary>
    public const string CanvasChangedEvent = "canvasChanged";

    /// <summary>
    /// Server-to-client event requesting that the receiving designer
    /// page switch its active selection to the given canvas. Payload:
    /// the canvas's id as a string. Lets an agent (via MCP
    /// <c>set_canvas_selected</c>) drive what the user is looking at —
    /// useful for "here's the canvas I just edited; watch this" flows.
    /// Distinct from <see cref="CanvasChangedEvent"/> because it's a UI
    /// directive, not a data-change notification.
    /// </summary>
    public const string SelectionRequestedEvent = "selectionRequested";
}
