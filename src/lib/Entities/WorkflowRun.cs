namespace LowlandTech.Domain.Entities;

public class WorkflowRun
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Branch { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// "Running", "Completed", "Failed", "Cancelled"
    /// </summary>
    public string Status { get; set; } = "Running";

    public string? CommitMessage { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation
    public Workflow Workflow { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
    public ICollection<WorkflowRunStep> Steps { get; set; } = [];
}
