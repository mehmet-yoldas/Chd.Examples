using Chd.StockTracking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chd.StockTracking.Api.Data;

public class StockTrackingDbContext : DbContext
{
    public StockTrackingDbContext(DbContextOptions<StockTrackingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<Selling> Sellings { get; set; }
    public DbSet<Inserting> Insertings { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerPayment> CustomerPayments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserType> UserTypes { get; set; }
    public DbSet<Unit> Units { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.BarcodeNumber).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Code).HasMaxLength(50);

            entity.HasOne(e => e.ProductCategory)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.ProductCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductCategory configuration
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
        });

        // Bill configuration
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("Bills");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Unit)
                .WithMany(e => e.Bills)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Bills)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany(e => e.Bills)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Selling configuration
        modelBuilder.Entity<Selling>(entity =>
        {
            entity.ToTable("Sellings");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.Sellings)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Bill)
                .WithMany(e => e.Sellings)
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inserting configuration
        modelBuilder.Entity<Inserting>(entity =>
        {
            entity.ToTable("Insertings");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.Insertings)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Unit)
                .WithMany(e => e.Insertings)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.Adress).HasMaxLength(250);
            entity.Property(e => e.TaxAdministration).HasMaxLength(250);
            entity.Property(e => e.Province).HasMaxLength(250);

            entity.HasOne(e => e.Unit)
                .WithMany(e => e.Customers)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CustomerPayment configuration
        modelBuilder.Entity<CustomerPayment>(entity =>
        {
            entity.ToTable("CustomerPayments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SignCode).HasMaxLength(50);

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.DeviceId).HasMaxLength(250);
            entity.Property(e => e.Culture).HasMaxLength(10);
            entity.Property(e => e.MobilePhone).HasMaxLength(50);

            entity.HasOne(e => e.Unit)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UserType)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.UserTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserType configuration
        modelBuilder.Entity<UserType>(entity =>
        {
            entity.ToTable("UserTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        // Unit configuration
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("Units");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
        });
    }
}
