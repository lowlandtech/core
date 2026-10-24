namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Seam that switches compose/nginx traffic to a newly-built plugin image
/// (or reverts it on rollback). Real implementations reload nginx/traefik config;
/// test doubles record calls.
/// SPEC2050 US20 SC01 / SC05.
/// </summary>
public interface IComposeSwitch
{
    /// <summary>
    /// Switches compose traffic for <paramref name="package"/> to the image identified
    /// by <paramref name="imageTag"/>.
    /// </summary>
    Task SwitchAsync(PluginRef package, string imageTag, CancellationToken ct);

    /// <summary>
    /// Reverts compose traffic for <paramref name="package"/> to the previous image.
    /// Called when the CI build fails (SC05).
    /// </summary>
    Task RevertAsync(PluginRef package, CancellationToken ct);
}
