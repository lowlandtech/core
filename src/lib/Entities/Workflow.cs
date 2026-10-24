namespace LowlandTech.Domain.Entities;

public class Workflow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid WorkspaceId { get; set; }
    public Guid? FlowId { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public int Order { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public Flow? Flow { get; set; }
}
