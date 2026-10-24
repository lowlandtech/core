namespace LowlandTech.Domain.Entities;

public class UserGroupMember
{
    public Guid Id { get; set; }
    public Guid UserGroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";

    public UserGroup UserGroup { get; set; } = null!;
}
