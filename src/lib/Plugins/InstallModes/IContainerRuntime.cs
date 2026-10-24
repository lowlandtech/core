namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Seam that pulls a sidecar image and starts a container, waiting for its
/// health check to report "running". Real implementations use the Docker /
/// docker-compose SDK; test doubles return immediately.
/// SPEC2050 US20 SC03.
/// </summary>
public interface IContainerRuntime
{
    /// <summary>
    /// Starts a container for <paramref name="package"/> and waits for its health
    /// check to pass.
    /// </summary>
    /// <returns>
    /// A <see cref="ContainerStartResult"/> with the container id and the endpoint
    /// other services should use to reach it.
    /// </returns>
    Task<ContainerStartResult> StartAsync(PluginRef package, CancellationToken ct);
}

/// <summary>
/// Result of a successful container start + health check.
/// SPEC2050 US20 SC03.
/// </summary>
public sealed record ContainerStartResult(string ContainerId, string ContainerEndpoint);
