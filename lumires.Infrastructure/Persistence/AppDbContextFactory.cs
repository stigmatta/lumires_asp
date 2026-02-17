using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<AppDbContextFactory>()
            .Build();

        var connectionString = configuration.GetConnectionString("db")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'db' not found in User Secrets.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            npgsqlOptions => { npgsqlOptions.MigrationsAssembly("lumires.Infrastructure"); });

        return new AppDbContext(optionsBuilder.Options);
    }
}