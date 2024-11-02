namespace LowlandTech.Plugin2.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}