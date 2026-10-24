namespace LowlandTech.Core.Plugins;

/// <summary>
/// Approval state of a <see cref="GraphMergeRequest"/>.
/// SPEC2050 US05.
/// </summary>
public enum MergeRequestStatus
{
    Open,
    Approved,
    Rejected,
}
