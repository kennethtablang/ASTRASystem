//Agent Supply & Transport Routing Assistant (ASTRA)
using ASTRASystem.Data;
using ASTRASystem.Helpers;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace ASTRASystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ??? QuestPDF Community License ???
            QuestPDF.Settings.License = LicenseType.Community;

            // 1. DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
            {
                // UPDATED: Strong password requirements
                opts.Password.RequireDigit = true;              // Must contain at least one number
                opts.Password.RequireLowercase = true;          // Must contain at least one lowercase letter
                opts.Password.RequireUppercase = true;          // Must contain at least one uppercase letter
                opts.Password.RequireNonAlphanumeric = true;    // Must contain at least one special character
                opts.Password.RequiredLength = 8;               // Minimum 8 characters
                opts.Password.RequiredUniqueChars = 1;          // At least 1 unique character

                // Optional: Configure lockout settings
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.AllowedForNewUsers = true;

                // Email confirmation not required for login (handled separately)
                opts.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // 3. JWT Settings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            // 4. Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.RequireHttpsMetadata = true;
                opts.SaveToken = true;
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,

                    // These are critical:
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    NameClaimType = JwtRegisteredClaimNames.Sub // this maps User.Identity.Name to user.Id
                };
            });


            // 5. Application services
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddScoped<ITokenService, Services.TokenService>();
            builder.Services.AddScoped<IAuthService, Services.AuthService>();
            builder.Services.AddScoped<IUserService, Services.UserService>();
            builder.Services.AddScoped<IProductService, Services.ProductService>();
            builder.Services.AddScoped<IStoreService, Services.StoreService>();
            builder.Services.AddScoped<IOrderService, Services.OrderService>();
            builder.Services.AddScoped<ITripService, Services.TripService>();
            builder.Services.AddScoped<IDeliveryService, Services.DeliveryService>();
            builder.Services.AddScoped<IPaymentService, Services.PaymentService>();
            builder.Services.AddScoped<IInvoiceService, Services.InvoiceService>();
            builder.Services.AddScoped<IWarehouseService, Services.WarehouseService>();
            builder.Services.AddScoped<IDistributorService, Services.DistributorService>();
            builder.Services.AddScoped<IAuditLogService, Services.AuditLogService>();
            builder.Services.AddScoped<INotificationService, Services.NotificationService>();
            builder.Services.AddScoped<IReportService, Services.ReportService>();
            builder.Services.AddScoped<IEmailService, Services.EmailService>();
            builder.Services.AddScoped<IFileStorageService, Services.FileStorageService>();
            builder.Services.AddScoped<IPdfService, Services.PdfService>();
            builder.Services.AddScoped<IExcelService, Services.ExcelService>();
            builder.Services.AddScoped<IBarcodeService, Services.BarcodeService>();

            // 6. Controllers + JSON options
            builder.Services
                .AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // 7. Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Agent Supply and Transport Routing Assistant (ASTRA) API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new()
                {
                    Description = "JWT Authorization header: Bearer {token}",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 8. CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendDev", builder =>
                {
                    builder.WithOrigins("http://localhost:5173")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Seed initial data
            using (var scope = app.Services.CreateScope())
            {
                await AstraSeeder.SeedAsync(scope.ServiceProvider);
            }

            // 9. Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agent Supply and Transport Routing Assistant (ASTRA) System v1"));
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agent Supply and Transport Routing Assistant (ASTRA) System v1"));

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontendDev");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
