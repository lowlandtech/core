namespace LowlandTech.Core.Abstractions;

public interface IGitService
{
    Task<GitBranchInfo> GetBranchInfoAsync(Guid workspaceId, string branchName, int maxCommits = 50, CancellationToken ct = default);
    Task StageFileAsync(Guid workspaceId, string branchName, string filePath, CancellationToken ct = default);
    Task UnstageFileAsync(Guid workspaceId, string branchName, string filePath, CancellationToken ct = default);
    Task StageAllAsync(Guid workspaceId, string branchName, CancellationToken ct = default);
    Task UnstageAllAsync(Guid workspaceId, string branchName, CancellationToken ct = default);
    Task CommitAsync(Guid workspaceId, string branchName, string message, CancellationToken ct = default);
}
