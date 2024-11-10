namespace LowlandTech.Plugin2.Domain.UseCases;

public class Plugin2UseCase(Plugin2Context context) : IUseCase
{
    public async Task SeedAsync()
    {
        // Create users
        var user1 = new User { Name = "Alice", Email = "alice@example.com" };
        var user2 = new User { Name = "Bob", Email = "bob@example.com" };
        var user3 = new User { Name = "Charlie", Email = "charlie@example.com" };
        context.Users.AddRange(user1, user2, user3);

        // Create products
        var product1 = new Product { Name = "Laptop", Price = 1000.00m };
        var product2 = new Product { Name = "Phone", Price = 500.00m };
        var product3 = new Product { Name = "Tablet", Price = 300.00m };
        context.Products.AddRange(product1, product2, product3);

        // Create orders
        var order1 = new Order { User = user1 };
        var order2 = new Order { User = user2 };
        var order3 = new Order { User = user3 };
        context.Orders.AddRange(order1, order2, order3);

        // Create order items
        var orderItem1 = new OrderItem { Order = order1, Product = product1, Quantity = 1 };
        var orderItem2 = new OrderItem { Order = order1, Product = product2, Quantity = 2 };
        var orderItem3 = new OrderItem { Order = order2, Product = product3, Quantity = 3 };
        var orderItem4 = new OrderItem { Order = order3, Product = product1, Quantity = 1 };
        context.OrderItems.AddRange(orderItem1, orderItem2, orderItem3, orderItem4);

        // Save changes to the database
        await context.SaveChangesAsync();
    }
}
