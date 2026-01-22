using lumires.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<UserNotification> UserNotifications  => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        //Movie
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
        });
        
        
        //Notification
        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.SenderId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.TargetId)
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.UserId, x.ReadAt });
        });

    }
}