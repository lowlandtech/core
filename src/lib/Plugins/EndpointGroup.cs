namespace LowlandTech.Core.Plugins;

/// <summary>
/// Base class a feature derives from to declare a group of HTTP endpoints.
/// SPEC2050 US11.
/// </summary>
/// <remarks>
/// Implementations override <see cref="GroupPrefix"/> and map their routes in
/// <see cref="MapEndpoints"/>. The host wires the group into the application
/// route table during startup (or on feature enable for hot-loaded features).
/// </remarks>
public abstract class EndpointGroup
{
    /// <summary>Route prefix the group is mounted at, e.g. <c>/api/auth</c>.</summary>
    public abstract string GroupPrefix { get; }

    /// <summary>OpenAPI tags applied to every endpoint in the group.</summary>
    public virtual string[] Tags => [];

    /// <summary>
    /// Authentication scheme name applied to every endpoint in the group, or
    /// <c>null</c> for an anonymous group.
    /// </summary>
    public virtual string? RequiredAuthenticationScheme => null;

    /// <summary>
    /// Authorization policy applied to every endpoint in the group, or
    /// <c>null</c> for no policy gate.
    /// </summary>
    public virtual string? RequiredPolicy => null;

    /// <summary>
    /// The entry point the host calls to mount the routes of the group.
    /// </summary>
    /// <param name="context">
    /// The host surface that owns the route table and the resolved services.
    /// </param>
    public abstract void MapEndpoints(IEndpointGroupContext context);
}
