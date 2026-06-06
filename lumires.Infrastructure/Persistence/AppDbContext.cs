using System.Diagnostics;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<ListFilm> ListFilms => Set<ListFilm>();
    public DbSet<Film> Films => Set<Film>();
    public DbSet<FilmLocalization> FilmLocalizations => Set<FilmLocalization>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<FilmsList> FilmsLists => Set<FilmsList>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<GenreLocalization> GenreLocalizations => Set<GenreLocalization>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();
    public DbSet<ReviewCommentLike> ReviewCommentLikes => Set<ReviewCommentLike>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<FilmCast> FilmCasts => Set<FilmCast>();
    public DbSet<FilmDirector> FilmDirectors => Set<FilmDirector>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<PersonLocalization> PersonsLocalizations => Set<PersonLocalization>();
    public DbSet<PersonDetail> PersonsDetails => Set<PersonDetail>();
    public DbSet<FilmsListLike> FilmsListLikes => Set<FilmsListLike>();
    public DbSet<UserFilmRating> UserFilmRatings => Set<UserFilmRating>();
    public DbSet<UserThread> Threads => Set<UserThread>();
    public DbSet<UserThreadComment> ThreadComments => Set<UserThreadComment>();
    public DbSet<UserThreadLike> ThreadLikes => Set<UserThreadLike>();
    public DbSet<UserThreadCommentLike> ThreadCommentLikes => Set<UserThreadCommentLike>();
    public DbSet<UsersRelationship> Relationships => Set<UsersRelationship>();
    public DbSet<WatchedFilm> WatchedFilms => Set<WatchedFilm>();
    public DbSet<SavedFilm> SavedFilms => Set<SavedFilm>();
    public DbSet<SavedList> SavedLists => Set<SavedList>();
    public DbSet<FilmLike> FilmLikes => Set<FilmLike>();
    public DbSet<FilmTag> FilmTags => Set<FilmTag>();
    public DbSet<ReviewTag> ReviewTags => Set<ReviewTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Debug.Assert(modelBuilder != null, nameof(modelBuilder) + " != null");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}