using ConnectDB.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Data;
public class AppDbContext : DbContext
{
    // Constructor này bắt buộc phải có để nhận Connection String từ Program.cs
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<Combo> Combos { get; set; }
    public DbSet<RevenueSnapshot> RevenueSnapshots { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Giúp PostgreSQL hiểu kiểu DateTime của .NET
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Seed dữ liệu Role mặc định (0: User, 1: Admin)
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 0, RoleName = "User" },
            new Role { RoleId = 1, RoleName = "Admin" }
        );
    }
}
