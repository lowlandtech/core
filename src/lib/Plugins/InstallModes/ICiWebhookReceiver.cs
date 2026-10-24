namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// Seam that waits for a CI pipeline to signal success or failure for the
/// given plugin package. Real implementations integrate with GitHub Actions,
/// Azure DevOps, etc. Test doubles return immediately.
/// SPEC2050 US20 SC01 / SC05.
/// </summary>
public interface ICiWebhookReceiver
{
    /// <summary>
    /// Waits for the CI webhook to report a result for <paramref name="package"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="CiBuildResult"/> containing the outcome and the published image tag
    /// (non-null on success).
    /// </returns>
    Task<CiBuildResult> WaitForBuildAsync(PluginRef package, CancellationToken ct);
}

/// <summary>
/// Terminal result from a CI pipeline run.
/// SPEC2050 US20 SC01 / SC05.
/// </summary>
public sealed record CiBuildResult(bool Succeeded, string? ImageTag, string? ErrorReason);
