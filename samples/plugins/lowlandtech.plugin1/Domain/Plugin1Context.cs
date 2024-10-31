namespace LowlandTech.Plugin1.Domain;

public class Plugin1Context(DbContextOptions<Plugin1Context> options) : DbContext(options)
{
    public DbSet<Plugin1Entity> Entities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!Database.IsSqlite())
            modelBuilder.HasDefaultSchema("Plugin1Schema");

        modelBuilder.Entity<Plugin1Entity>().ToTable("Plugin1Entities");
    }
}
