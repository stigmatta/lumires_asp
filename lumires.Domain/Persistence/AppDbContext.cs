using System.Diagnostics;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Domain.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MovieLocalization> MovieLocalizations => Set<MovieLocalization>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Debug.Assert(modelBuilder != null, nameof(modelBuilder) + " != null");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}