namespace LowlandTech.Core.Plugins;

/// <summary>
/// The host surface that an <see cref="EndpointGroup"/> maps its routes onto.
/// </summary>
/// <remarks>
/// The contract names no web framework type. The host implements this over its
/// own route table and its own resolved services, and the group maps its
/// routes through it, so that a producer implements one member and a host
/// reads one member.
/// </remarks>
public interface IEndpointGroupContext
{
    /// <summary>The services the host resolved for the group.</summary>
    IServiceProvider Services { get; }

    /// <summary>Maps one route of the group on the route table that the host owns.</summary>
    /// <param name="method">The HTTP method of the route, for example <c>GET</c>.</param>
    /// <param name="pattern">The route pattern of the route, relative to the group prefix.</param>
    /// <param name="handler">The handler of the route.</param>
    void Map(string method, string pattern, Delegate handler);
}
