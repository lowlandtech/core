namespace LowlandTech.Core;

public class WorkspaceSettings
{
    public string Root { get; set; } = OperatingSystem.IsWindows()
        ? "e:/workspaces"
        : "/root/workspaces";

    /// <summary>
    /// Resolves the output path for a workspace branch worktree.
    /// Pattern: {Root}/{workspaceId}/{branch}
    /// </summary>
    public string GetWorktreePath(Guid workspaceId, string branch)
        => Path.Combine(Root, workspaceId.ToString(), branch).Replace('\\', '/');

    /// <summary>
    /// Resolves the root path for a workspace (contains all branch worktrees).
    /// Pattern: {Root}/{workspaceId}
    /// </summary>
    public string GetWorkspacePath(Guid workspaceId)
        => Path.Combine(Root, workspaceId.ToString()).Replace('\\', '/');
}
