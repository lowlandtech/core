namespace LowlandTech.Plugin2.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}