using Microsoft.EntityFrameworkCore;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

   public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Inventory> Inventory { get; set; } = null!;
    public DbSet<Movement> Movements { get; set; } = null!;
}
