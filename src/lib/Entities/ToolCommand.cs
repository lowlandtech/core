namespace LowlandTech.Domain.Entities;

public class ToolCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Template string with {{parameter}} placeholders processed by TinyToolEngine.
    /// e.g. "commit -m \"{{message}}\"", "worktree add \"{{path}}\" -b {{branch}}"
    /// </summary>
    public string Template { get; set; } = string.Empty;

    /// <summary>
    /// Working directory template. e.g. "{{workspacePath}}/{{branch}}"
    /// </summary>
    public string WorkingDirTemplate { get; set; } = string.Empty;

    /// <summary>
    /// JSON schema or simple key-value definition of parameters.
    /// e.g. { "message": "string", "branch": "string", "force": "bool" }
    /// </summary>
    public string Parameters { get; set; } = "{}";

    /// <summary>
    /// Optional group for organizing commands (e.g. "worktree", "branch", "remote", "flow")
    /// </summary>
    public string Group { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid ToolId { get; set; }
    public Tool Tool { get; set; } = null!;
}
