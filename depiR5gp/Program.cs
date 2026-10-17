using depiR5gp.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cross-platform DB Configuration (Windows / Linux Docker / Azure)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

var baseConnection = builder.Configuration.GetConnectionString("DefaultConnection") 
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2. Auto-apply EF Core Migrations on startup (Works locally & on Azure!)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
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