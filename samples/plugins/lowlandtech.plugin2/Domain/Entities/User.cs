namespace LowlandTech.Plugin2.Domain.Entities;

// Entity Classes
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
}