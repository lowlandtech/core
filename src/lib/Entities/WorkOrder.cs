namespace LowlandTech.Domain.Entities;

public enum WorkOrderStatus
{
    Pending,
    AwaitingPayment,
    Paid,
    Provisioning,
    Provisioned,
    Complete,
    Failed,
    Abandoned
}

public class WorkOrder
{
    public Guid            Id              { get; set; } = Guid.NewGuid();
    public string          Email           { get; set; } = "";
    public string          Plan            { get; set; } = "";
    public string          Interval        { get; set; } = "";
    public string          WorkspaceName   { get; set; } = "";
    public string          Organisation    { get; set; } = "";
    public string?         LogoUrl         { get; set; }
    public string?         Theme           { get; set; }
    public WorkOrderStatus Status          { get; set; } = WorkOrderStatus.Pending;
    public string?         Data            { get; set; }           // JSON blob — workspace config
    public string?         StripeSessionId { get; set; }
    public string?         ActivationToken { get; set; }
    public DateTimeOffset  CreatedAt       { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt     { get; set; }
    public DateTimeOffset? ActivatedAt     { get; set; }
}
