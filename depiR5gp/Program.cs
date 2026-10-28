using depiR5gp.Data;
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

// Register Repositories & Unit of Work
builder.Services.AddScoped(typeof(depiR5gp.Repositories.Interfaces.IGenericRepository<>), typeof(depiR5gp.Repositories.Implementations.GenericRepository<>));
builder.Services.AddScoped<depiR5gp.Repositories.Interfaces.IUnitOfWork, depiR5gp.Repositories.Implementations.UnitOfWork>();
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

var app = builder.Build();

// 2. Auto-apply EF Core Migrations on startup (Works locally & on Azure!)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("⚠️ Could not connect to SQL Server to auto-apply migrations on startup ({Message}). The application will continue running in offline/mock mode.", ex.Message);
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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();