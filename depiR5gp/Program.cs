using depiR5gp.Data;
using depiR5gp.Data.Seeding;
using depiR5gp.Models.Users;
using Microsoft.AspNetCore.Identity;
using depiR5gp.Services.Implementations;
using depiR5gp.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cross-platform DB Configuration (Windows / Linux Docker / Azure)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

var baseConnection = builder.Configuration.GetConnectionString("DefaultConnection") 
                     ?? "Server=.;Database=depiR5gp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

var connectionBuilder = new SqlConnectionStringBuilder(baseConnection);
var dbServer = builder.Configuration["DbServer"];
var dbUser = builder.Configuration["DbUser"];
var dbPassword = builder.Configuration["DbPassword"];

if (!string.IsNullOrEmpty(dbServer))
{
    connectionBuilder.IntegratedSecurity = false;
    connectionBuilder.DataSource = dbServer;
    connectionBuilder.UserID = dbUser;
    connectionBuilder.Password = dbPassword;
    connectionBuilder.Encrypt = false;
    connectionBuilder.TrustServerCertificate = true;
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionBuilder.ConnectionString));

builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Validate the auth cookie's security stamp on every request so that suspending/deactivating
// an account (which rotates the stamp) revokes the user's active sessions immediately.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.Zero);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
// Register Repositories & Unit of Work
builder.Services.AddScoped(typeof(depiR5gp.Repositories.Interfaces.IGenericRepository<>), typeof(depiR5gp.Repositories.Implementations.GenericRepository<>));
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IUnitOfWork, depiR5gp.Repositories.Implementations.UnitOfWork>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IUserRepository, depiR5gp.Repositories.Implementations.UserRepository>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IChefRepository, depiR5gp.Repositories.Implementations.ChefRepository>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IDishRepository, depiR5gp.Repositories.Implementations.DishRepository>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.ICartRepository, depiR5gp.Repositories.Implementations.CartRepository>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IOrderRepository, depiR5gp.Repositories.Implementations.OrderRepository>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IReviewRepository, depiR5gp.Repositories.Implementations.ReviewRepository>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IConversationRepository, depiR5gp.Repositories.Implementations.ConversationRepository>();
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.ISupportRepository, depiR5gp.Repositories.Implementations.SupportRepository>();
//file services
builder.Services.AddScoped<IFileService, LocalFileService>();
builder.Services.AddControllersWithViews();
//chef Services
builder.Services.AddScoped<depiR5gp.Services.Interfaces.IChefService, depiR5gp.Services.Implementations.ChefService>();
builder.Services.AddScoped<IDishService, DishService>();

// Database Seeding Infrastructure (Development only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSeeders(builder.Configuration);
}

var app = builder.Build();

// 2. Auto-apply EF Core Migrations & seed database on startup (Works locally & on Azure!)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Reconcile schema drift from previous migrations if database already has AppUsers
    try
    {
        db.Database.ExecuteSqlRaw(@"
            IF COL_LENGTH('AppUsers', 'role') IS NOT NULL
            BEGIN
                DECLARE @defName sysname;
                SELECT @defName = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AppUsers]') AND [c].[name] = N'role');
                IF @defName IS NOT NULL EXEC(N'ALTER TABLE [AppUsers] DROP CONSTRAINT [' + @defName + '];');
                ALTER TABLE [AppUsers] DROP COLUMN [role];
            END;

            IF COL_LENGTH('AppUsers', 'is_suspended') IS NOT NULL
            BEGIN
                ALTER TABLE [AppUsers] DROP COLUMN [is_suspended];
            END;

            IF OBJECT_ID(N'[AppUsers]', N'U') IS NOT NULL AND COL_LENGTH('AppUsers', 'status') IS NULL
            BEGIN
                ALTER TABLE [AppUsers] ADD [status] nvarchar(20) NOT NULL DEFAULT 'Active';
            END;

            IF OBJECT_ID(N'[AppUsers]', N'U') IS NOT NULL AND COL_LENGTH('AppUsers', 'points_balance') IS NULL
            BEGIN
                ALTER TABLE [AppUsers] ADD [points_balance] int NOT NULL DEFAULT 0;
            END;

            IF OBJECT_ID(N'UserStatusHistories', N'U') IS NULL AND OBJECT_ID(N'[AppUsers]', N'U') IS NOT NULL
            BEGIN
                CREATE TABLE [UserStatusHistories] (
                    [id] int NOT NULL IDENTITY,
                    [user_id] int NOT NULL,
                    [changed_by_user_id] int NULL,
                    [status] nvarchar(20) NOT NULL,
                    [changed_at] datetime2 NOT NULL,
                    [reason] nvarchar(200) NULL,
                    CONSTRAINT [PK_UserStatusHistories] PRIMARY KEY ([id]),
                    CONSTRAINT [CK_UserStatusHistories_status] CHECK (status IN ('Active', 'Suspended', 'Deleted')),
                    CONSTRAINT [FK_UserStatusHistories_AppUsers_changed_by_user_id] FOREIGN KEY ([changed_by_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_UserStatusHistories_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION
                );
            END;
        ");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogDebug("Schema reconciliation check: {Message}", ex.Message);
    }

    db.Database.Migrate();

    // Ensure essential Identity roles exist in all environments (Production, Staging, Development)
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    foreach (var role in AppRoles.All)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("⚠️ Could not auto-apply migrations on startup ({Message}). Continuing...", ex.Message);
}

if (app.Environment.IsDevelopment())
{
    try
    {
        await app.SeedDatabaseAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("⚠️ Could not seed database on startup ({Message}). The application will continue running in offline/mock mode.", ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();