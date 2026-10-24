namespace LowlandTech.Domain.Abstractions;

/// <summary>
/// Server-side notifier for canvas-mutation events. Implementations
/// fan out to connected designer clients (typically via a SignalR hub)
/// so the `/admin/canvas` page refreshes when a canvas is changed by
/// any caller — UI, REST, or MCP.
///
/// Lives in <c>LowlandTech.Domain.Abstractions</c> rather than the API
/// project so <c>ContainerService</c> can depend on it without dragging
/// SignalR into <c>src/domain</c>. The API project provides the
/// concrete implementation (<c>CanvasNotifier</c> wrapping
/// <c>IHubContext&lt;CanvasHub&gt;</c>).
/// </summary>
public interface ICanvasNotifier
{
    Task NotifyCanvasChangedAsync(Guid containerId, CancellationToken ct = default);

    /// <summary>
    /// Push a "select this canvas" directive to all connected designer
    /// clients. Backs the MCP <c>set_canvas_selected</c> tool — lets an
    /// agent steer what the user is looking at.
    /// </summary>
    Task RequestSelectionAsync(Guid containerId, CancellationToken ct = default);
}
