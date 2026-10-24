namespace LowlandTech.Domain.Entities;

public class WorkflowRunStep
{
    public Guid Id { get; set; }
    public Guid WorkflowRunId { get; set; }
    public Guid StepId { get; set; }
    public int SortOrder { get; set; }

    /// <summary>
    /// For Data="Table" steps, which table this execution was for.
    /// Null for Data="Workspace" steps.
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// "Ran", "Skipped", "Unchanged", "Error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 of rendered content (Template) or rendered command text (Command/ToolCommand).
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Resolved output path (Template) or full command text (ToolCommand/Command).
    /// </summary>
    public string? ResolvedValue { get; set; }

    /// <summary>
    /// Process exit code for command steps. Null for Template steps.
    /// </summary>
    public int? ExitCode { get; set; }

    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; }

    // Navigation
    public WorkflowRun WorkflowRun { get; set; } = null!;
    public Step Step { get; set; } = null!;
    public DataTable? Table { get; set; }
}
