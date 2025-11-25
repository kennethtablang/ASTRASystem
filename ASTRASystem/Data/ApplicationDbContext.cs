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

            // ---- Decimal precision for monetary values ----
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.Tax)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Invoice>()
                .Property(i => i.TaxAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Store>()
                .Property(s => s.CreditLimit)
                .HasColumnType("decimal(18,2)");

            // ---- Decimal precision for geographic coordinates (Lat/Lng) ----
            // Format: decimal(precision, scale)
            // For coordinates: decimal(10,7) gives 7 decimal places (~1.1cm accuracy)
            // Range: -999.9999999 to 999.9999999 (more than enough for lat/lng)

            builder.Entity<Warehouse>()
                .Property(w => w.Latitude)
                .HasColumnType("decimal(10,7)");

            builder.Entity<Warehouse>()
                .Property(w => w.Longitude)
                .HasColumnType("decimal(10,7)");

            builder.Entity<DeliveryPhoto>()
                .Property(d => d.Lat)
                .HasColumnType("decimal(10,7)");

            builder.Entity<DeliveryPhoto>()
                .Property(d => d.Lng)
                .HasColumnType("decimal(10,7)");

            // ---- Relationships & delete behavior ----
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // ---- Indexes for query performance ----
            builder.Entity<Order>()
                .HasIndex(o => o.Status);

            builder.Entity<Order>()
                .HasIndex(o => o.WarehouseId);

            builder.Entity<Trip>()
                .HasIndex(t => t.DispatcherId);

            builder.Entity<Store>()
                .HasIndex(s => new { s.Barangay, s.City });

            // ---- String length constraints ----
            builder.Entity<ApplicationUser>()
                .Property(u => u.FirstName)
                .HasMaxLength(150);

            builder.Entity<ApplicationUser>()
                .Property(u => u.LastName)
                .HasMaxLength(150);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(entity.CreatedById))
                    {
                        entity.CreatedById = "system";
                    }
                }

                entity.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(entity.UpdatedById))
                {
                    entity.UpdatedById = "system";
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}