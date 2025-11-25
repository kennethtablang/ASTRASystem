using ASTRASystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Distributor> Distributors { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripAssignment> TripAssignments { get; set; }
        public DbSet<DeliveryPhoto> DeliveryPhotos { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---- Default string lengths for Identity keys (if needed) ----
            // Identity uses nvarchar(450) by default for string keys; above models use that expectation.

            // ---- Decimal precision (explicit to avoid provider differences) ----
            builder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.Tax).HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.Total).HasColumnType("decimal(18,2)");
            builder.Entity<OrderItem>().Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Entity<Invoice>().Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Entity<Invoice>().Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");

            // ---- Relationships & delete behavior (restrict deletes to avoid accidental cascade) ----
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // keep cascade for items when order deleted

            builder.Entity<Order>()
                .HasMany(o => o.DeliveryPhotos)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Trip>()
                .HasMany(t => t.Assignments)
                .WithOne(a => a.Trip)
                .HasForeignKey(a => a.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            builder.Entity<Order>().HasIndex(o => new { o.Status });
            builder.Entity<Order>().HasIndex(o => new { o.WarehouseId });
            builder.Entity<Trip>().HasIndex(t => new { t.DispatcherId });
            builder.Entity<Store>().HasIndex(s => new { s.Barangay, s.City });

            // Limit string lengths explicitly where used in models (defensive)
            builder.Entity<ApplicationUser>().Property(u => u.FirstName).HasMaxLength(150);
            builder.Entity<ApplicationUser>().Property(u => u.LastName).HasMaxLength(150);

            // Additional model customizations can go here (composite keys, alternate keys, etc.)
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

}
