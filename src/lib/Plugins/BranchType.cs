namespace LowlandTech.Core.Plugins;

/// <summary>
/// Determines the target PostgreSQL database a branch will merge into.
/// SPEC2050 US05.
/// </summary>
public enum BranchType
{
    /// <summary>Targets the develop database.</summary>
    Feature,

    /// <summary>Targets the main database directly; auto-creates a back-merge MR to develop.</summary>
    Hotfix,

    /// <summary>Cut from develop, targets main for release promotion.</summary>
    Release,
}
