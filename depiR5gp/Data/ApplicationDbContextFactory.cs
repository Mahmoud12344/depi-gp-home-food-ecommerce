using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace depiR5gp.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddUserSecrets(typeof(ApplicationDbContextFactory).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var baseConnection = configuration.GetConnectionString("DefaultConnection") 
                             ?? "Server=.;Database=depiR5gp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var connectionBuilder = new SqlConnectionStringBuilder(baseConnection);
        var dbServer = configuration["DbServer"];
        var dbUser = configuration["DbUser"];
        var dbPassword = configuration["DbPassword"];

        if (!string.IsNullOrEmpty(dbServer))
        {
            connectionBuilder.IntegratedSecurity = false;
            connectionBuilder.DataSource = dbServer;
            connectionBuilder.UserID = dbUser;
            connectionBuilder.Password = dbPassword;
            connectionBuilder.Encrypt = false;
            connectionBuilder.TrustServerCertificate = true;
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionBuilder.ConnectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
