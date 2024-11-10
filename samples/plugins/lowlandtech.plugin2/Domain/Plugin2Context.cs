namespace LowlandTech.Plugin2.Domain;

[DbContextId("177f3a1a-e1db-4d85-b66f-fcb18b0005d1")]
public class Plugin2Context(DbContextOptions<Plugin2Context> options) : DbContext(options)
{
    [DbSetId("a9b4dfe7-4f29-4c9e-a5e8-cf546edb785b")]
    public DbSet<User> Users { get; set; }

    [DbSetId("d1a9c88b-e2b4-498e-8455-b1dca8e979c7")]
    public DbSet<Product> Products { get; set; }

    [DbSetId("b5e7e9c2-2e3f-476d-b930-36bbf43d56f1")]
    public DbSet<Order> Orders { get; set; }

    [DbSetId("c4f8ed12-1a45-45f5-a5c1-dbd4e53f9d9b")]
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define relationships and configurations if needed
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);
    }
}