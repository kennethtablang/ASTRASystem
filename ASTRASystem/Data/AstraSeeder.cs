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

            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Database migrate failed during seeding (may be fine in some environments).");
            }

            // 1) Create roles
            foreach (var roleName in DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var r = new IdentityRole(roleName);
                    await roleManager.CreateAsync(r);
                    logger?.LogInformation("Created role {role}", roleName);
                }
            }

            // 2) Helper function to ensure user exists
            async Task<ApplicationUser> EnsureUser(string userName, string email, string firstName, string lastName, string role, long? distributorId = null, long? warehouseId = null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
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
                    PhoneNumberConfirmed = true,
                    IsApproved = true
                };

                var result = await userManager.CreateAsync(user, "Admin#123");
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

            // 3) Create Admin user FIRST (needed for CreatedById)
            var adminUser = await userManager.FindByEmailAsync("admin@astra.local");
            if (adminUser == null)
            {
                adminUser = await EnsureUser("admin", "admin@astra.local", "System", "Admin", "Admin");
            }

            // 4) Seed a default Distributor & Warehouse
            var distributor = await db.Distributors.FirstOrDefaultAsync();
            if (distributor == null)
            {
                distributor = new Distributor
                {
                    Name = "Demo Distributor",
                    ContactPhone = "09171234567",
                    Address = "Main Warehouse, Demo City",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
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
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Warehouses.Add(warehouse);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded Warehouse: {name}", warehouse.Name);
            }

            // 5) Seed Categories (10 categories)
            List<Category> categories = null;
            if (!await db.Categories.AnyAsync())
            {
                categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Beverages",
                        Description = "Drinks, juices, and beverages",
                        Color = "#3B82F6",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Snacks",
                        Description = "Chips, crackers, and snacks",
                        Color = "#F59E0B",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Canned Goods",
                        Description = "Canned foods and preserved items",
                        Color = "#EF4444",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Dairy Products",
                        Description = "Milk, cheese, and dairy items",
                        Color = "#8B5CF6",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Household Items",
                        Description = "Cleaning supplies and household goods",
                        Color = "#10B981",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Personal Care",
                        Description = "Hygiene and personal care products",
                        Color = "#EC4899",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Condiments & Sauces",
                        Description = "Sauces, seasonings, and condiments",
                        Color = "#F97316",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Frozen Foods",
                        Description = "Frozen products and ice cream",
                        Color = "#06B6D4",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Bakery & Bread",
                        Description = "Bread, pastries, and baked goods",
                        Color = "#FBBF24",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    },
                    new Category
                    {
                        Name = "Confectionery",
                        Description = "Candies, chocolates, and sweets",
                        Color = "#A855F7",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    }
                };

                db.Categories.AddRange(categories);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} categories", categories.Count);
            }
            else
            {
                // Load existing categories for product seeding
                categories = await db.Categories.ToListAsync();
            }

            // 5.5) Seed Products (100 products) and Inventory
            if (!await db.Products.AnyAsync() && categories != null && categories.Any())
            {
                var random = new Random(42); // Fixed seed for reproducible data
                var products = new List<Product>();
                int skuCounter = 1000;

                // Helper function to create products
                void AddProducts(string categoryName, params (string name, decimal price, string unit, bool perishable)[] items)
                {
                    var category = categories.FirstOrDefault(c => c.Name == categoryName);
                    if (category == null) return;

                    foreach (var item in items)
                    {
                        skuCounter++;
                        products.Add(new Product
                        {
                            Name = item.name,
                            Sku = $"SKU-{skuCounter:D6}",
                            Barcode = $"8{random.Next(100000000, 999999999):D11}",
                            CategoryId = category.Id,
                            Price = item.price,
                            UnitOfMeasure = item.unit,
                            IsPerishable = item.perishable,
                            IsBarcoded = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = adminUser.Id,
                            UpdatedById = adminUser.Id
                        });
                    }
                }

                // BEVERAGES (15 products)
                AddProducts("Beverages",
                    ("Coca-Cola 1.5L", 45m, "bottle", false),
                    ("Pepsi 1.5L", 43m, "bottle", false),
                    ("Sprite 1.5L", 43m, "bottle", false),
                    ("Royal 1.5L", 40m, "bottle", false),
                    ("C2 Green Tea 1L", 25m, "bottle", false),
                    ("Nestea Iced Tea 1L", 25m, "bottle", false),
                    ("Zest-O Orange 200ml", 12m, "pack", false),
                    ("Tang Orange 25g", 8m, "sachet", false),
                    ("Nescafe 3-in-1 Original", 7m, "sachet", false),
                    ("Great Taste White Coffee", 7m, "sachet", false),
                    ("Milo Powder 33g", 10m, "sachet", false),
                    ("Bear Brand 1L", 85m, "bottle", true),
                    ("Energy Drink 250ml", 35m, "can", false),
                    ("Bottled Water 500ml", 15m, "bottle", false),
                    ("Pineapple Juice 1L", 55m, "bottle", false)
                );

                // SNACKS (15 products)
                AddProducts("Snacks",
                    ("Chippy BBQ 110g", 25m, "pack", false),
                    ("Piattos Cheese 85g", 28m, "pack", false),
                    ("Nova Barbecue 78g", 22m, "pack", false),
                    ("Oishi Prawn Crackers 90g", 20m, "pack", false),
                    ("Roller Coaster 85g", 18m, "pack", false),
                    ("Clover Chips Cheese 85g", 20m, "pack", false),
                    ("Jack n Jill Peanuts 100g", 35m, "pack", false),
                    ("Boy Bawang Cornick 100g", 30m, "pack", false),
                    ("Cheese Curls 75g", 22m, "pack", false),
                    ("Potato Chips Salt 60g", 25m, "pack", false),
                    ("Crackers Skyflakes 10s", 28m, "pack", false),
                    ("Fita 10s", 25m, "pack", false),
                    ("Cream-O Vanilla 10s", 30m, "pack", false),
                    ("Rebisco Choco Mallows", 35m, "pack", false),
                    ("Pretzels 100g", 40m, "pack", false)
                );

                // CANNED GOODS (12 products)
                AddProducts("Canned Goods",
                    ("Century Tuna Flakes 155g", 38m, "can", false),
                    ("555 Sardines 155g", 22m, "can", false),
                    ("Mega Sardines 155g", 20m, "can", false),
                    ("Ligo Sardines 155g", 24m, "can", false),
                    ("Argentina Corned Beef 175g", 45m, "can", false),
                    ("Purefoods Corned Beef 150g", 42m, "can", false),
                    ("Spam Classic 340g", 165m, "can", false),
                    ("Libby's Vienna Sausage 130g", 28m, "can", false),
                    ("CDO Liver Spread 85g", 18m, "can", false),
                    ("Del Monte Tomato Sauce 250g", 25m, "can", false),
                    ("Jolly Mushroom 400g", 55m, "can", false),
                    ("Young's Town Sardines 155g", 22m, "can", false)
                );

                // DAIRY PRODUCTS (8 products)
                AddProducts("Dairy Products",
                    ("Alaska Evaporated Milk 370ml", 35m, "can", true),
                    ("Carnation Condensed Milk 300ml", 40m, "can", true),
                    ("Nestle Fresh Milk 1L", 95m, "bottle", true),
                    ("Eden Cheese 165g", 75m, "bar", true),
                    ("Nestle All Purpose Cream 250ml", 42m, "pack", true),
                    ("Anchor Milk Powder 900g", 385m, "pack", false),
                    ("Yakult 5-pack", 45m, "pack", true),
                    ("Yogurt Drink 180ml", 25m, "bottle", true)
                );

                // HOUSEHOLD ITEMS (10 products)
                AddProducts("Household Items",
                    ("Tide Powder 120g", 18m, "pack", false),
                    ("Ariel Powder 120g", 20m, "pack", false),
                    ("Surf Powder 120g", 15m, "pack", false),
                    ("Downy Concentrate 45ml", 8m, "sachet", false),
                    ("Joy Dishwashing Liquid 250ml", 35m, "bottle", false),
                    ("Domex Bleach 500ml", 38m, "bottle", false),
                    ("Ajax Cleanser 350g", 28m, "bottle", false),
                    ("Zonrox Bleach 500ml", 40m, "bottle", false),
                    ("Baygon Spray 300ml", 95m, "can", false),
                    ("Lysol Disinfectant 225ml", 88m, "bottle", false)
                );

                // PERSONAL CARE (12 products)
                AddProducts("Personal Care",
                    ("Safeguard Bar Soap 135g", 35m, "bar", false),
                    ("Palmolive Bar Soap 135g", 32m, "bar", false),
                    ("Colgate Toothpaste 150g", 58m, "tube", false),
                    ("Close-Up Toothpaste 150g", 55m, "tube", false),
                    ("Oral-B Toothbrush", 45m, "piece", false),
                    ("Head & Shoulders Shampoo 170ml", 88m, "bottle", false),
                    ("Pantene Shampoo 170ml", 85m, "bottle", false),
                    ("Cream Silk Conditioner 180ml", 75m, "sachet", false),
                    ("Dove Body Wash 200ml", 125m, "bottle", false),
                    ("Rexona Deo Roll-On 40ml", 65m, "bottle", false),
                    ("Whisper Sanitary Napkin 8s", 45m, "pack", false),
                    ("Johnson's Baby Powder 100g", 68m, "bottle", false)
                );

                // CONDIMENTS & SAUCES (10 products)
                AddProducts("Condiments & Sauces",
                    ("Silver Swan Soy Sauce 385ml", 28m, "bottle", false),
                    ("Datu Puti Vinegar 385ml", 18m, "bottle", false),
                    ("UFC Catsup 320g", 42m, "bottle", false),
                    ("Mama Sita's Oyster Sauce 405g", 58m, "bottle", false),
                    ("Maggi Magic Sarap 8g", 5m, "sachet", false),
                    ("Knorr Pork Cube 60g", 25m, "pack", false),
                    ("McCormick Seasoning Mix", 15m, "pack", false),
                    ("Datu Puti Soy Sauce 1L", 65m, "bottle", false),
                    ("Papa Catsup 550g", 55m, "bottle", false),
                    ("Lorins Chili Garlic 210g", 48m, "bottle", false)
                );

                // FROZEN FOODS (8 products)
                AddProducts("Frozen Foods",
                    ("Tender Juicy Hotdog 1kg", 185m, "pack", true),
                    ("CDO Chicken Franks 1kg", 165m, "pack", true),
                    ("Purefoods Corned Beef 175g", 48m, "can", false),
                    ("Magnolia Chicken 1kg", 195m, "pack", true),
                    ("Selecta Ice Cream 1.5L", 245m, "tub", true),
                    ("Nestlé Ice Cream 1.5L", 235m, "tub", true),
                    ("Fish Fillet 500g", 145m, "pack", true),
                    ("Frozen Vegetables Mix 400g", 85m, "pack", true)
                );

                // BAKERY & BREAD (5 products)
                AddProducts("Bakery & Bread",
                    ("Gardenia Classic White Bread", 52m, "loaf", true),
                    ("Gardenia Wheat Bread", 55m, "loaf", true),
                    ("Tasty Bread Loaf", 45m, "loaf", true),
                    ("Pandesal 10pcs", 35m, "pack", true),
                    ("Ensaymada 6pcs", 65m, "pack", true)
                );

                // CONFECTIONERY (5 products)
                AddProducts("Confectionery",
                    ("Choc-Nut 24pcs", 48m, "pack", false),
                    ("Goya Chocolate 10s", 25m, "pack", false),
                    ("Storck Candy 125g", 58m, "pack", false),
                    ("Mentos Mint Roll", 15m, "roll", false),
                    ("Chupa Chups 12s", 72m, "pack", false)
                );

                // Add all products to database
                db.Products.AddRange(products);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} products across {categories} categories", 
                    products.Count, categories.Count);

                // Create Inventory for all products in the warehouse
                if (warehouse != null)
                {
                    var inventoryRecords = new List<Inventory>();
                    foreach (var product in products)
                    {
                        // Random quantity between 50 and 500
                        var quantity = random.Next(50, 501);
                        var reorderLevel = random.Next(20, 51);

                        inventoryRecords.Add(new Inventory
                        {
                            WarehouseId = warehouse.Id,
                            ProductId = product.Id,
                            StockLevel = quantity,
                            ReorderLevel = reorderLevel,
                            LastRestocked = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = adminUser.Id,
                            UpdatedById = adminUser.Id
                        });
                    }

                    db.Inventories.AddRange(inventoryRecords);
                    await db.SaveChangesAsync();
                    logger?.LogInformation("Seeded inventory for {count} products in warehouse {warehouse}",
                        inventoryRecords.Count, warehouse.Name);
                }
            }

            // 6) Seed Cities and Barangays (Pangasinan)
            City alaminosCity = null;
            Barangay poblacionBarangay = null;

            if (!await db.Cities.AnyAsync())
            {
                // Seed Alaminos City
                alaminosCity = new City
                {
                    Name = "Alaminos",
                    Province = "Pangasinan",
                    Region = "Ilocos Region",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Cities.Add(alaminosCity);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded City: {name}", alaminosCity.Name);

                // Seed Barangays for Alaminos City (39 barangays)
                var alaminosBarangays = new[]
                {
                    "Alos", "Amandiego", "Amangbangan", "Balangobong", "Balayang",
                    "Baleyadaan", "Bisocol", "Bolaney", "Bued", "Cabatuan",
                    "Cayucay", "Dulacac", "Inerangan", "Landoc", "Linmansangan",
                    "Lucap", "Maawi", "Macatiw", "Magsaysay", "Mona",
                    "Palamis", "Pandan", "Pangapisan", "Poblacion", "Pocalpocal",
                    "Pogo", "Polo", "Quibuar", "Sabangan", "San Antonio",
                    "San Jose", "San Roque", "San Vicente", "Santa Maria", "Tanaytay",
                    "Tangcarang", "Tawintawin", "Telbang", "Victoria"
                };

                foreach (var barangayName in alaminosBarangays)
                {
                    var barangay = new Barangay
                    {
                        Name = barangayName,
                        CityId = alaminosCity.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    };
                    db.Barangays.Add(barangay);

                    if (barangayName == "Poblacion")
                    {
                        poblacionBarangay = barangay;
                    }
                }
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} barangays for {city}", alaminosBarangays.Length, alaminosCity.Name);

                // Seed Burgos
                var burgosCity = new City
                {
                    Name = "Burgos",
                    Province = "Pangasinan",
                    Region = "Ilocos Region",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Cities.Add(burgosCity);
                await db.SaveChangesAsync();

                var burgosBarangays = new[]
                {
                    "Anapao", "Cacayasen", "Concordia", "Don Matias", "Ilio-ilio",
                    "Papallasen", "Poblacion", "Pogoruac", "San Miguel", "San Pascual",
                    "San Vicente", "Sapa Grande", "Sapa Pequeña", "Tambacan"
                };

                foreach (var barangayName in burgosBarangays)
                {
                    db.Barangays.Add(new Barangay
                    {
                        Name = barangayName,
                        CityId = burgosCity.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    });
                }
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} barangays for {city}", burgosBarangays.Length, burgosCity.Name);

                // Seed Bani
                var baniCity = new City
                {
                    Name = "Bani",
                    Province = "Pangasinan",
                    Region = "Ilocos Region",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Cities.Add(baniCity);
                await db.SaveChangesAsync();

                var baniBarangays = new[]
                {
                    "Ambabaay", "Aporao", "Arwas", "Ballag", "Banog Norte",
                    "Banog Sur", "Calabeng", "Centro Toma", "Colayo", "Dacap Norte",
                    "Dacap Sur", "Garrita", "Luac", "Macabit", "Masidem",
                    "Poblacion", "Quinaoayanan", "Ranao", "Ranom Iloco", "San Jose",
                    "San Miguel", "San Simon", "San Vicente", "Tiep", "Tipor",
                    "Tugui Grande", "Tugui Norte"
                };

                foreach (var barangayName in baniBarangays)
                {
                    db.Barangays.Add(new Barangay
                    {
                        Name = barangayName,
                        CityId = baniCity.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    });
                }
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} barangays for {city}", baniBarangays.Length, baniCity.Name);

                // Seed Sual
                var sualCity = new City
                {
                    Name = "Sual",
                    Province = "Pangasinan",
                    Region = "Ilocos Region",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Cities.Add(sualCity);
                await db.SaveChangesAsync();

                var sualBarangays = new[]
                {
                    "Baquioen", "Baybay Norte", "Baybay Sur", "Bolaoen", "Cabalitian",
                    "Calumbuyan", "Camagsingalan", "Caoayan", "Capantolan", "Macaycayawan",
                    "Paitan East", "Paitan West", "Pangascasan", "Poblacion", "Santo Domingo",
                    "Seselangen", "Sioasio East", "Sioasio West", "Victoria"
                };

                foreach (var barangayName in sualBarangays)
                {
                    db.Barangays.Add(new Barangay
                    {
                        Name = barangayName,
                        CityId = sualCity.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    });
                }
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} barangays for {city}", sualBarangays.Length, sualCity.Name);

                // Seed Agno
                var agnoCity = new City
                {
                    Name = "Agno",
                    Province = "Pangasinan",
                    Region = "Ilocos Region",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Cities.Add(agnoCity);
                await db.SaveChangesAsync();

                var agnoBarangays = new[]
                {
                    "Allabon", "Aloleng", "Bangan-Oda", "Baruan", "Boboy",
                    "Cayungnan", "Dangley", "Gayusan", "Macaboboni", "Magsaysay",
                    "Namatucan", "Patar", "Poblacion East", "Poblacion West", "San Juan",
                    "Tupa", "Viga"
                };

                foreach (var barangayName in agnoBarangays)
                {
                    db.Barangays.Add(new Barangay
                    {
                        Name = barangayName,
                        CityId = agnoCity.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    });
                }
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} barangays for {city}", agnoBarangays.Length, agnoCity.Name);

                // Seed Mabini
                var mabiniCity = new City
                {
                    Name = "Mabini",
                    Province = "Pangasinan",
                    Region = "Ilocos Region",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Cities.Add(mabiniCity);
                await db.SaveChangesAsync();

                var mabiniBarangays = new[]
                {
                    "Bacnit", "Barlo", "Caabiangaan", "Cabanaetan", "Cabinuangan",
                    "Calzada", "Caranglaan", "De Guzman", "Luna", "Magalong",
                    "Nibaliw", "Patar", "Poblacion", "San Pedro", "Tagudin",
                    "Villacorta"
                };

                foreach (var barangayName in mabiniBarangays)
                {
                    db.Barangays.Add(new Barangay
                    {
                        Name = barangayName,
                        CityId = mabiniCity.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = adminUser.Id,
                        UpdatedById = adminUser.Id
                    });
                }
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {count} barangays for {city}", mabiniBarangays.Length, mabiniCity.Name);
            }
            else
            {
                // Load existing city and barangay for store creation
                alaminosCity = await db.Cities.FirstOrDefaultAsync(c => c.Name == "Alaminos");
                if (alaminosCity != null)
                {
                    poblacionBarangay = await db.Barangays
                        .FirstOrDefaultAsync(b => b.Name == "Poblacion" && b.CityId == alaminosCity.Id);
                }
            }

            // 7) Seed Stores with City/Barangay relationships
            if (!await db.Stores.AnyAsync())
            {
                var store = new Store
                {
                    Name = "Sari-Sari Store - Poblacion Alaminos",
                    CityId = alaminosCity?.Id,
                    BarangayId = poblacionBarangay?.Id,
                    OwnerName = "Tito Manny",
                    Phone = "09171230000",
                    CreditLimit = 2000m,
                    PreferredPaymentMethod = "Cash",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = adminUser.Id,
                    UpdatedById = adminUser.Id
                };
                db.Stores.Add(store);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded sample Store {name} in {barangay}, {city}",
                    store.Name, poblacionBarangay?.Name, alaminosCity?.Name);

                // Seed additional sample stores
                if (alaminosCity != null)
                {
                    var lucapBarangay = await db.Barangays
                        .FirstOrDefaultAsync(b => b.Name == "Lucap" && b.CityId == alaminosCity.Id);

                    var additionalStores = new[]
                    {
                        new Store
                        {
                            Name = "Mini Mart - Lucap Beach",
                            CityId = alaminosCity.Id,
                            BarangayId = lucapBarangay?.Id,
                            OwnerName = "Maria Santos",
                            Phone = "09171231111",
                            CreditLimit = 5000m,
                            PreferredPaymentMethod = "Cash",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = adminUser.Id,
                            UpdatedById = adminUser.Id
                        },
                        new Store
                        {
                            Name = "Corner Store - Bued",
                            CityId = alaminosCity.Id,
                            OwnerName = "Juan Dela Cruz",
                            Phone = "09171232222",
                            CreditLimit = 3000m,
                            PreferredPaymentMethod = "GCash",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = adminUser.Id,
                            UpdatedById = adminUser.Id
                        }
                    };

                    db.Stores.AddRange(additionalStores);
                    await db.SaveChangesAsync();
                    logger?.LogInformation("Seeded {count} additional stores", additionalStores.Length);
                }
            }

            // 8) Create other users (after distributor & warehouse exist)

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

            // 9) Optional: create a sample order to illustrate relationships (only if none exist)
            if (!await db.Orders.AnyAsync() && await db.Products.AnyAsync())
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = agent.Id,
                    UpdatedById = agent.Id
                };

                var line = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = agent.Id,
                    UpdatedById = agent.Id
                };

                order.Items.Add(line);
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded sample Order {orderId} for store {store}", order.Id, store.Name);
            }

            // 10) Data Fix: Ensure all orders have a DistributorId (for existing data compatibility)
            var ordersWithoutDistributor = await db.Orders
                .Where(o => o.DistributorId == null)
                .ToListAsync();

            if (ordersWithoutDistributor.Any())
            {
                foreach (var o in ordersWithoutDistributor)
                {
                    o.DistributorId = distributor.Id;
                    o.WarehouseId = warehouse.Id; // Also fix warehouse if missing
                }
                await db.SaveChangesAsync();
                logger?.LogInformation("Fixed {count} orders with missing DistributorId", ordersWithoutDistributor.Count);
            }

            logger?.LogInformation("AstraSeeder completed successfully.");
        }
    }
}