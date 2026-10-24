namespace LowlandTech.Domain.Entities;

public class Step
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid FlowId { get; set; }
    public int SortOrder { get; set; }

    /// <summary>
    /// Step type: "Template" (render a template), "ToolCommand" (execute a tool command), "Command" (inline command)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Data scope: "Workspace" (run once), "Table" (run per table)
    /// </summary>
    public string Data { get; set; } = "Workspace";

    /// <summary>
    /// References the entity for this step type:
    /// - Type="Template" → TypeId is a Template.Id
    /// - Type="ToolCommand" → TypeId is a ToolCommand.Id
    /// - Type="Command" → TypeId is null (inline command in Content)
    /// </summary>
    public Guid? TypeId { get; set; }

    /// <summary>
    /// Snapshot of the command/template content at step creation time.
    /// For Template: the output file path template.
    /// For ToolCommand: snapshot of the tool command template.
    /// For Command: the inline command text.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// NCalc expression evaluated before executing this step.
    /// If the expression evaluates to false, the step is skipped.
    /// Context variables from the workspace/table are available.
    /// Empty or null means the step always runs.
    /// </summary>
    public string Conditions { get; set; } = string.Empty;

    // ── Navigation (kept for backward compat with Template type) ──
    public Flow Flow { get; set; } = null!;

    // Note: No direct nav to Template or ToolCommand — resolved via TypeId + Type at runtime
}
