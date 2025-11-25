using ASTRASystem.Enum;
using ASTRASystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Data
{
    public static class AstraSeeder
    {
        public static readonly string[] DefaultRoles = new[]
        {
            "Admin",
            "DistributorAdmin",
            "Agent",
            "Dispatcher",
            "Accountant"
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger = null)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // 1) Ensure DB is created and migrations applied (optional - use in dev only)
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Database migrate failed during seeding (may be fine in some environments).");
            }

            // 2) Create roles
            foreach (var roleName in DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var r = new IdentityRole(roleName);
                    await roleManager.CreateAsync(r);
                    logger?.LogInformation("Created role {role}", roleName);
                }
            }

            // Helper to create users
            async Task<ApplicationUser> EnsureUser(string userName, string email, string firstName, string lastName, string role, long? distributorId = null, long? warehouseId = null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    // Ensure in role
                    if (!await userManager.IsInRoleAsync(user, role))
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                    return user;
                }

                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    DistributorId = distributorId,
                    WarehouseId = warehouseId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                // NOTE: change the password before using in production
                var result = await userManager.CreateAsync(user, "P@ssw0rd!");
                if (!result.Succeeded)
                {
                    var errs = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger?.LogError("Error creating user {email}: {errs}", email, errs);
                    throw new Exception($"Failed to create user {email}: {errs}");
                }

                await userManager.AddToRoleAsync(user, role);
                logger?.LogInformation("Created user {email} as {role}", email, role);
                return user;
            }

            // 3) Seed a default Distributor & Warehouse if none exist
            var distributor = await db.Distributors.FirstOrDefaultAsync();
            if (distributor == null)
            {
                distributor = new Distributor
                {
                    Name = "Demo Distributor",
                    ContactPhone = "09171234567",
                    Address = "Main Warehouse, Demo City",
                    CreatedAt = DateTime.UtcNow
                };
                db.Distributors.Add(distributor);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded Distributor: {name}", distributor.Name);
            }

            var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.DistributorId == distributor.Id);
            if (warehouse == null)
            {
                warehouse = new Warehouse
                {
                    DistributorId = distributor.Id,
                    Name = "Demo Warehouse A",
                    Address = "Barangay Central, Demo City",
                    Latitude = 14.5995m,
                    Longitude = 120.9842m,
                    CreatedAt = DateTime.UtcNow
                };
                db.Warehouses.Add(warehouse);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded Warehouse: {name}", warehouse.Name);
            }

            // 4) Seed sample product(s) if none exist
            if (!await db.Products.AnyAsync())
            {
                var seedProducts = new List<Product>
                {
                    new Product { Sku = "RICE-5KG", Name = "Rice 5kg", Category = "Staples", Price = 250m, UnitOfMeasure = "bag" },
                    new Product { Sku = "NOODLES-5", Name = "Instant Noodles - 5pcs", Category = "Groceries", Price = 75m, UnitOfMeasure = "pack" },
                    new Product { Sku = "COOKINGOIL-1L", Name = "Cooking Oil 1L", Category = "Staples", Price = 120m, UnitOfMeasure = "bottle" }
                };
                db.Products.AddRange(seedProducts);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} products", seedProducts.Count);
            }

            // 5) Seed sample store (retailer)
            if (!await db.Stores.AnyAsync())
            {
                var s = new Store
                {
                    Name = "Sari-Sari Store - San Isidro",
                    Barangay = "San Isidro",
                    City = "Demo City",
                    OwnerName = "Tito Manny",
                    Phone = "09171230000",
                    CreditLimit = 2000m,
                    CreatedAt = DateTime.UtcNow
                };
                db.Stores.Add(s);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded sample Store {name}", s.Name);
            }

            // 6) Create users: Admin, DistributorAdmin, Agent, Dispatcher, Accountant
            // Admin (super user)
            var adminUser = await userManager.FindByEmailAsync("admin@astra.local");
            if (adminUser == null)
            {
                adminUser = await EnsureUser("admin", "admin@astra.local", "System", "Admin", "Admin");
            }

            // Distributor admin (tied to distributor)
            var distAdmin = await userManager.FindByEmailAsync("distadmin@demo.local");
            if (distAdmin == null)
            {
                distAdmin = await EnsureUser("distadmin", "distadmin@demo.local", "Distributor", "Admin", "DistributorAdmin", distributor.Id);
            }

            // Agent
            var agent = await userManager.FindByEmailAsync("agent1@demo.local");
            if (agent == null)
            {
                agent = await EnsureUser("agent1", "agent1@demo.local", "Agent", "One", "Agent");
            }

            // Dispatcher
            var dispatcher = await userManager.FindByEmailAsync("dispatcher1@demo.local");
            if (dispatcher == null)
            {
                dispatcher = await EnsureUser("dispatcher1", "dispatcher1@demo.local", "Dispatch", "One", "Dispatcher", distributor.Id, warehouse.Id);
            }

            // Accountant
            var accountant = await userManager.FindByEmailAsync("accountant1@demo.local");
            if (accountant == null)
            {
                accountant = await EnsureUser("accountant1", "accountant1@demo.local", "Account", "One", "Accountant");
            }

            // 7) Optional: create a sample order to illustrate relationships (only if none exist)
            if (!await db.Orders.AnyAsync())
            {
                var product = await db.Products.FirstAsync();
                var store = await db.Stores.FirstAsync();

                var order = new Order
                {
                    StoreId = store.Id,
                    AgentId = agent.Id,
                    DistributorId = distributor.Id,
                    WarehouseId = warehouse.Id,
                    Status = OrderStatus.Pending,
                    Priority = false,
                    ScheduledFor = DateTime.UtcNow,
                    SubTotal = product.Price,
                    Tax = 0m,
                    Total = product.Price,
                    CreatedAt = DateTime.UtcNow
                };

                var line = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    CreatedAt = DateTime.UtcNow
                };

                order.Items.Add(line);
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded sample Order {orderId} for store {store}", order.Id, store.Name);
            }

            logger?.LogInformation("AstraSeeder completed.");
        }
    }
}
