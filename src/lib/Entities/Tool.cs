namespace LowlandTech.Domain.Entities;

public class Tool
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Path or name of the executable (e.g. "git", "dotnet", "npm")</summary>
    public string Executable { get; set; } = string.Empty;

    /// <summary>Arguments to check if the tool is installed (e.g. "--version")</summary>
    public string CheckCommand { get; set; } = "--version";

    /// <summary>Optional command to auto-install the tool (e.g. "winget install Git.Git")</summary>
    public string? InstallCommand { get; set; }

    /// <summary>"shell" for external CLI tools, "internal" for built-in executors like TinyToolEngine</summary>
    public string ExecutorType { get; set; } = "shell";

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ToolCommand> Commands { get; set; } = [];
}
