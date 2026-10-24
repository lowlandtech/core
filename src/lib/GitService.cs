
namespace LowlandTech.Core;

public record GitCommit(string Hash, string ShortHash, string Message, string Author, string Date, string? Refs, string[] ParentHashes);
public record GitFileStatus(string Status, string Path, bool IsStaged);
public record GitCommitFile(string Status, string Path);
public record GitBranchInfo(
    bool Exists,
    bool IsInitialized,
    string CurrentBranch,
    IReadOnlyList<GitCommit> Commits,
    IReadOnlyList<GitService.GitRemote> Remotes,
    IReadOnlyList<GitFileStatus> StagedFiles,
    IReadOnlyList<GitFileStatus> UncommittedFiles);

public class GitService
{
    private readonly WorkspaceSettings _settings;
    public WorkspaceSettings Settings => _settings;

    public GitService(WorkspaceSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Ensures a branch worktree exists. Smart setup logic:
    /// 1. If the directory already has a .git, just use it
    /// 2. If a remote is provided and reachable, clone it and checkout the branch
    /// 3. Otherwise, init a new repo with the branch name as the initial branch
    /// </summary>
    public async Task<string> EnsureBranchAsync(Guid workspaceId, string branchName, string? remote = null)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        Directory.CreateDirectory(path);

        // Already initialized? Verify it's a valid repo (has at least one commit)
        if (Directory.Exists(Path.Combine(path, ".git")) || File.Exists(Path.Combine(path, ".git")))
        {
            var (verifyExit, _) = await RunGitAsync(path, "rev-parse HEAD");
            if (verifyExit == 0)
                return path;
            // Invalid/empty repo — clean up and recreate
            try { Directory.Delete(path, recursive: true); } catch { }
            Directory.CreateDirectory(path);
        }

        // Try clone from remote
        if (!string.IsNullOrWhiteSpace(remote))
        {
            var (cloneExit, _) = await RunGitAsync(".", $"clone --branch {branchName} --single-branch \"{remote}\" \"{path}\"");
            if (cloneExit == 0)
                return path;

            // Remote exists but branch doesn't — clone default then create branch
            var (cloneDefaultExit, _) = await RunGitAsync(".", $"clone \"{remote}\" \"{path}\"");
            if (cloneDefaultExit == 0)
            {
                await RunGitAsync(path, $"checkout -b {branchName}");
                return path;
            }
        }

        // Check if any existing worktree/repo exists in the workspace — fork from it
        var wsRoot = _settings.GetWorkspacePath(workspaceId);
        var normalizedPath = Path.GetFullPath(path);
        if (Directory.Exists(wsRoot))
        {
            // Prefer the main repo (has .git directory, not file) — that's the one owning all worktrees
            var forkFrom = Directory.GetDirectories(wsRoot)
                .Where(d => !string.Equals(Path.GetFullPath(d), normalizedPath, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(d => Directory.Exists(Path.Combine(d, ".git")));

            // If no main repo, try any linked worktree (has .git file) — read its gitdir to find the main repo
            if (forkFrom is null)
            {
                var linkedWorktree = Directory.GetDirectories(wsRoot)
                    .Where(d => !string.Equals(Path.GetFullPath(d), normalizedPath, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(d => File.Exists(Path.Combine(d, ".git")));

                if (linkedWorktree is not null)
                {
                    // Read .git file to find the main repo's gitdir
                    var gitFileContent = await File.ReadAllTextAsync(Path.Combine(linkedWorktree, ".git"));
                    var gitDir = gitFileContent.Replace("gitdir:", "").Trim();
                    // gitdir points to .git/worktrees/{name} — go up to .git, then up to the repo root
                    var mainGitDir = Path.GetFullPath(Path.Combine(linkedWorktree, gitDir, "..", ".."));
                    if (Directory.Exists(mainGitDir))
                        forkFrom = Path.GetDirectoryName(mainGitDir) ?? linkedWorktree;
                }
            }

            if (forkFrom is not null)
            {
                // worktree add fails if the target directory is not empty — clean it first
                if (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length == 0)
                {
                    // Empty dir is fine, worktree add handles it
                }
                else if (Directory.Exists(path))
                {
                    try { Directory.Delete(path, recursive: true); } catch { }
                    Directory.CreateDirectory(path);
                }

                // Prune stale worktree refs that might block reuse of the branch name
                await RunGitAsync(forkFrom, "worktree prune");

                // Check if the branch already exists in the source repo
                var (branchCheckExit, _) = await RunGitAsync(forkFrom, $"rev-parse --verify {branchName}");
                var (worktreeExit, worktreeErr) = branchCheckExit == 0
                    ? await RunGitAsync(forkFrom, $"worktree add \"{path}\" {branchName}")     // existing branch
                    : await RunGitAsync(forkFrom, $"worktree add \"{path}\" -b {branchName}"); // new branch

                if (worktreeExit == 0)
                    return path;
            }
        }

        // Last resort: init fresh with an initial commit so worktrees can fork from it
        await RunGitAsync(path, $"init --initial-branch={branchName}");
        await RunGitAsync(path, "config user.email foundry@lowlandtech.com");
        await RunGitAsync(path, "config user.name Foundry");

        // Create .gitignore and initial commit — worktree add requires at least one commit
        var gitignorePath = Path.Combine(path, ".gitignore");
        if (!File.Exists(gitignorePath))
            await File.WriteAllTextAsync(gitignorePath, "bin/\nobj/\n.vs/\n*.user\n");
        await RunGitAsync(path, "add -A");
        await RunGitAsync(path, "commit -m \"chore: initial commit\"");

        return path;
    }

    /// <summary>
    /// Gets the branch info: current branch, commit log, and uncommitted files.
    /// </summary>
    public async Task<GitBranchInfo> GetBranchInfoAsync(Guid workspaceId, string branchName, int maxCommits = 50)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);

        if (!Directory.Exists(path))
            return new GitBranchInfo(false, false, branchName, [], [], [], []);

        var hasGit = Directory.Exists(Path.Combine(path, ".git")) || File.Exists(Path.Combine(path, ".git"));
        if (!hasGit)
            return new GitBranchInfo(true, false, branchName, [], [], [], []);

        // Current branch
        var (_, currentBranch) = await RunGitAsync(path, "rev-parse --abbrev-ref HEAD");
        currentBranch = currentBranch.Trim();

        // Commit log
        var commits = await GetCommitsAsync(path, maxCommits);

        // Remotes
        var remotes = await GetRemotesAsync(workspaceId, branchName);

        // All files from status
        var allFiles = await GetStatusAsync(path);
        var staged = allFiles.Where(f => f.IsStaged).ToList();
        var unstaged = allFiles.Where(f => !f.IsStaged).ToList();

        return new GitBranchInfo(true, true, currentBranch, commits, remotes, staged, unstaged);
    }

    public async Task<IReadOnlyList<GitCommit>> GetCommitsAsync(string repoPath, int maxCommits = 50)
    {
        var format = "%H|%h|%s|%an|%ci|%D|%P";
        var (exitCode, output) = await RunGitAsync(repoPath, $"log --format=\"{format}\" -n {maxCommits}");
        if (exitCode != 0 || string.IsNullOrWhiteSpace(output))
            return [];

        var commits = new List<GitCommit>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|', 7);
            if (parts.Length >= 5)
            {
                var refs = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) ? parts[5] : null;
                var parentHashes = parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6])
                    ? parts[6].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    : [];
                commits.Add(new GitCommit(
                    Hash: parts[0],
                    ShortHash: parts[1],
                    Message: parts[2],
                    Author: parts[3],
                    Date: parts[4],
                    Refs: refs,
                    ParentHashes: parentHashes));
            }
        }
        return commits;
    }

    public async Task<IReadOnlyList<GitCommitFile>> GetCommitFilesAsync(string repoPath, string commitHash)
    {
        var (exitCode, output) = await RunGitAsync(repoPath, $"diff-tree --no-commit-id -r --name-status {commitHash}");
        if (exitCode != 0 || string.IsNullOrWhiteSpace(output))
            return [];

        var files = new List<GitCommitFile>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t', 2);
            if (parts.Length == 2)
                files.Add(new GitCommitFile(parts[0], parts[1]));
        }
        return files;
    }

    public async Task<IReadOnlyList<GitFileStatus>> GetStatusAsync(string repoPath)
    {
        var (exitCode, output) = await RunGitAsync(repoPath, "status --porcelain");
        if (exitCode != 0 || string.IsNullOrWhiteSpace(output))
            return [];

        var files = new List<GitFileStatus>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length >= 3)
            {
                // Porcelain format: XY PATH where X=index, Y=worktree
                var indexStatus = line[0];
                var worktreeStatus = line[1];
                var filePath = line[3..].Trim();

                // Staged: X is not ' ' or '?' (has index change)
                if (indexStatus != ' ' && indexStatus != '?')
                    files.Add(new GitFileStatus(indexStatus.ToString(), filePath, IsStaged: true));

                // Unstaged: Y is not ' ' (has working tree change), or untracked (??)
                if (worktreeStatus != ' ' || (indexStatus == '?' && worktreeStatus == '?'))
                {
                    var status = indexStatus == '?' ? "??" : worktreeStatus.ToString();
                    files.Add(new GitFileStatus(status, filePath, IsStaged: false));
                }
            }
        }
        return files;
    }

    /// <summary>
    /// Renames a branch worktree: for linked worktrees, removes the old and creates a new one
    /// with the new branch name. For standalone repos, renames the branch and moves the directory.
    /// </summary>
    public async Task RenameWorktreeAsync(Guid workspaceId, string oldName, string newName)
    {
        var oldPath = _settings.GetWorktreePath(workspaceId, oldName);
        var newPath = _settings.GetWorktreePath(workspaceId, newName);
        if (!Directory.Exists(oldPath)) return;
        if (Directory.Exists(newPath)) return; // target already exists

        var gitPath = Path.Combine(oldPath, ".git");
        if (File.Exists(gitPath))
        {
            // Linked worktree — remove old worktree and create new one with new branch name
            var wsRoot = _settings.GetWorkspacePath(workspaceId);
            var mainPath = Directory.GetDirectories(wsRoot)
                .FirstOrDefault(d => d != oldPath && Directory.Exists(Path.Combine(d, ".git")));

            if (mainPath is not null)
            {
                // Get the current commit to preserve state
                var (_, commitHash) = await RunGitAsync(oldPath, "rev-parse HEAD");
                commitHash = commitHash.Trim();

                // Remove the old worktree
                await RunGitAsync(mainPath, $"worktree remove \"{oldPath}\" --force");

                // Delete the old branch name if it exists
                await RunGitAsync(mainPath, $"branch -D {oldName}");

                // Create new worktree with new branch name from the same commit
                if (!string.IsNullOrEmpty(commitHash) && commitHash.Length >= 7)
                    await RunGitAsync(mainPath, $"worktree add \"{newPath}\" -b {newName} {commitHash}");
                else
                    await RunGitAsync(mainPath, $"worktree add \"{newPath}\" -b {newName}");

                return;
            }
        }

        // Standalone repo — rename the branch and move the directory
        await RunGitAsync(oldPath, $"branch -m {oldName} {newName}");
        try { Directory.Move(oldPath, newPath); } catch { }
    }

    /// <summary>
    /// Deletes a branch worktree from disk. If it's a git worktree, removes it properly first.
    /// </summary>
    public async Task DeleteWorktreeAsync(Guid workspaceId, string branchName)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        var wsRoot = _settings.GetWorkspacePath(workspaceId);

        // Find the main repo (owns all worktrees)
        string? mainPath = null;
        if (Directory.Exists(wsRoot))
        {
            mainPath = Directory.GetDirectories(wsRoot)
                .Where(d => d != path)
                .FirstOrDefault(d => Directory.Exists(Path.Combine(d, ".git")));
        }

        if (Directory.Exists(path))
        {
            var gitPath = Path.Combine(path, ".git");
            if (File.Exists(gitPath) && mainPath is not null)
            {
                // Linked worktree — remove via git
                await RunGitAsync(mainPath, $"worktree remove \"{path}\" --force");
            }
            else
            {
                // Standalone repo — delete the directory
                try { Directory.Delete(path, recursive: true); } catch { }
            }
        }

        // Clean up the branch ref from the main repo
        if (mainPath is not null)
        {
            // Prune stale worktree references first — git won't delete a branch
            // it thinks is still checked out in a (now-removed) worktree
            await RunGitAsync(mainPath, "worktree prune");

            // Now delete the branch ref
            var (exitCode, _) = await RunGitAsync(mainPath, $"branch -D {branchName}");

            if (exitCode != 0)
            {
                // If branch -D fails (e.g. branch is current in another worktree),
                // try to delete the ref directly as a fallback
                await RunGitAsync(mainPath, $"update-ref -d refs/heads/{branchName}");
            }
        }

        // Also ensure the directory is truly gone (worktree remove can leave remnants)
        if (Directory.Exists(path))
        {
            try { Directory.Delete(path, recursive: true); } catch { }
        }
    }

    // ── Remote Management ──

    public record GitRemote(string Name, string FetchUrl, string PushUrl);

    public async Task<IReadOnlyList<GitRemote>> GetRemotesAsync(Guid workspaceId, string branchName)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        var (exitCode, output) = await RunGitAsync(path, "remote -v");
        if (exitCode != 0 || string.IsNullOrWhiteSpace(output))
            return [];

        var remotes = new Dictionary<string, (string? Fetch, string? Push)>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t', 2);
            if (parts.Length < 2) continue;
            var name = parts[0];
            var urlAndType = parts[1].Split(' ', 2);
            var url = urlAndType[0];
            var type = urlAndType.Length > 1 ? urlAndType[1].Trim('(', ')') : "fetch";

            if (!remotes.ContainsKey(name))
                remotes[name] = (null, null);

            var current = remotes[name];
            remotes[name] = type == "push"
                ? (current.Fetch, url)
                : (url, current.Push);
        }

        return remotes.Select(kv => new GitRemote(kv.Key, kv.Value.Fetch ?? "", kv.Value.Push ?? "")).ToList();
    }

    public async Task AddRemoteAsync(Guid workspaceId, string branchName, string remoteName, string url)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        await RunGitAsync(path, $"remote add {remoteName} \"{url}\"");
    }

    public async Task RemoveRemoteAsync(Guid workspaceId, string branchName, string remoteName)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        await RunGitAsync(path, $"remote remove {remoteName}");
    }

    public async Task RenameRemoteAsync(Guid workspaceId, string branchName, string oldName, string newName)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        await RunGitAsync(path, $"remote rename {oldName} {newName}");
    }

    public async Task SetRemoteUrlAsync(Guid workspaceId, string branchName, string remoteName, string url)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        await RunGitAsync(path, $"remote set-url {remoteName} \"{url}\"");
    }

    public async Task FetchRemoteAsync(Guid workspaceId, string branchName, string remoteName)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        await RunGitAsync(path, $"fetch {remoteName}");
    }

    public async Task PushAsync(Guid workspaceId, string branchName, string remoteName, string? targetBranch = null)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        var target = targetBranch ?? branchName;
        await RunGitAsync(path, $"push {remoteName} {target}");
    }

    public async Task PullAsync(Guid workspaceId, string branchName, string remoteName)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        await RunGitAsync(path, $"pull {remoteName} {branchName}");
    }

    public async Task StageFileAsync(string repoPath, string filePath)
    {
        await RunGitAsync(repoPath, $"add \"{filePath}\"");
    }

    public async Task UnstageFileAsync(string repoPath, string filePath)
    {
        await RunGitAsync(repoPath, $"restore --staged \"{filePath}\"");
    }

    public async Task StageAllAsync(Guid workspaceId, string branchName)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        await RunGitAsync(path, "add -A");
    }

    public async Task CommitAsync(Guid workspaceId, string branchName, string message)
    {
        var path = _settings.GetWorktreePath(workspaceId, branchName);
        // Stage all first, then commit
        await RunGitAsync(path, "add -A");
        var escapedMessage = message.Replace("\"", "\\\"");
        await RunGitAsync(path, $"commit -m \"{escapedMessage}\"");
    }

    // ── GitFlow with Worktrees ──

    public enum FlowType { Feature, Release, Hotfix }

    /// <summary>
    /// Starts a GitFlow branch as a worktree.
    /// Feature/Release fork from develop, Hotfix forks from main.
    /// </summary>
    public async Task<string> StartFlowAsync(Guid workspaceId, FlowType type, string name)
    {
        var prefix = type switch
        {
            FlowType.Feature => "feature",
            FlowType.Release => "release",
            FlowType.Hotfix => "hotfix",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        var branchName = $"{prefix}/{name}";
        var sourceBranch = type == FlowType.Hotfix ? "main" : "develop";

        // Ensure the source branch worktree exists
        await EnsureBranchAsync(workspaceId, sourceBranch);

        var sourcePath = _settings.GetWorktreePath(workspaceId, sourceBranch);
        var targetPath = _settings.GetWorktreePath(workspaceId, branchName);

        if (Directory.Exists(targetPath))
        {
            // Already exists — verify it's valid
            var (verifyExit, _) = await RunGitAsync(targetPath, "rev-parse HEAD");
            if (verifyExit == 0) return targetPath;
            try { Directory.Delete(targetPath, recursive: true); } catch { }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

        // Prune stale worktree refs
        await RunGitAsync(sourcePath, "worktree prune");

        // Create worktree branching from source
        var (exit, _) = await RunGitAsync(sourcePath, $"worktree add \"{targetPath}\" -b {branchName}");
        if (exit != 0)
        {
            // Branch might already exist — try checkout
            await RunGitAsync(sourcePath, $"worktree add \"{targetPath}\" {branchName}");
        }

        return targetPath;
    }

    /// <summary>
    /// Finishes a GitFlow branch: merges into target(s) and removes the worktree.
    /// Feature → develop, Release → main + develop, Hotfix → main + develop.
    /// </summary>
    public async Task FinishFlowAsync(Guid workspaceId, FlowType type, string branchName)
    {
        var developPath = _settings.GetWorktreePath(workspaceId, "develop");
        var mainPath = _settings.GetWorktreePath(workspaceId, "main");
        var flowPath = _settings.GetWorktreePath(workspaceId, branchName);

        switch (type)
        {
            case FlowType.Feature:
                // Merge into develop
                await RunGitAsync(developPath, $"merge {branchName} --no-ff -m \"Merge {branchName} into develop\"");
                break;

            case FlowType.Release:
            case FlowType.Hotfix:
                // Merge into main
                await EnsureBranchAsync(workspaceId, "main");
                await RunGitAsync(mainPath, $"merge {branchName} --no-ff -m \"Merge {branchName} into main\"");
                // Also merge into develop
                await RunGitAsync(developPath, $"merge {branchName} --no-ff -m \"Merge {branchName} into develop\"");
                break;
        }

        // Remove the flow worktree and branch
        await DeleteWorktreeAsync(workspaceId, branchName);
    }

    /// <summary>Runs an arbitrary shell command in the specified directory.</summary>
    public static async Task<(int ExitCode, string Output)> RunCommandAsync(string workingDir, string command)
    {
        // Split executable from arguments
        var parts = command.Split(' ', 2);
        var exe = parts[0];
        var args = parts.Length > 1 ? parts[1] : "";
        return await RunProcessAsync(workingDir, exe, args);
    }

    private static Task<(int ExitCode, string Output)> RunGitAsync(string workingDir, string arguments)
        => RunProcessAsync(workingDir, "git", arguments);

    private static async Task<(int ExitCode, string Output)> RunProcessAsync(string workingDir, string fileName, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
                return (-1, "Failed to start git process");

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                Console.Error.WriteLine($"[git {arguments}] exit={process.ExitCode}: {error.Trim()}");

            return (process.ExitCode, output);
        }
        catch (Exception ex)
        {
            return (-1, ex.Message);
        }
    }
}
