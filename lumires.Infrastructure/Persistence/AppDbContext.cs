using System.Diagnostics;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<CollectionMovie> CollectionMovies => Set<CollectionMovie>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MovieLocalization> MovieLocalizations => Set<MovieLocalization>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<GenreLocalization> GenreLocalizations => Set<GenreLocalization>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();
    public DbSet<ReviewCommentLike> ReviewCommentLikes => Set<ReviewCommentLike>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<MovieCast> MovieCasts => Set<MovieCast>();
    public DbSet<MovieDirector> MovieDirectors => Set<MovieDirector>();
    public DbSet<Person> Persons => Set<Person>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Debug.Assert(modelBuilder != null, nameof(modelBuilder) + " != null");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}