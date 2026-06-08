using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace lumires.Core.Abstractions.Data;

public interface IAppDbContext : IDisposable, IAsyncDisposable
{
    DbSet<Film> Films { get; }
    DbSet<FilmLocalization> FilmLocalizations { get; }

    DbSet<UserNotification> UserNotifications { get; }
    DbSet<FilmsList> FilmsLists { get; }
    DbSet<ListFilm> ListFilms { get; }
    DbSet<User> Users { get; }
    DbSet<Genre> Genres { get; }
    DbSet<GenreLocalization> GenreLocalizations { get; }
    DbSet<Review> Reviews { get; }
    DbSet<ReviewComment> ReviewComments { get; }
    DbSet<ReviewLike> ReviewLikes { get; }
    DbSet<ReviewCommentLike> ReviewCommentLikes { get; }
    DbSet<FilmsListLike> FilmsListLikes { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<FilmCast> FilmCasts { get; }
    DbSet<FilmDirector> FilmDirectors { get; }
    DbSet<Person> Persons { get; }
    DbSet<PersonLocalization> PersonsLocalizations { get; }
    DbSet<PersonDetail> PersonsDetails { get; }
    DbSet<UserFilmRating> UserFilmRatings { get; }
    DbSet<UserThread> Threads { get; }
    DbSet<UserThreadComment> ThreadComments { get; }
    DbSet<UserThreadLike> ThreadLikes { get; }
    DbSet<UserThreadCommentLike> ThreadCommentLikes { get; }
    DbSet<UsersRelationship> Relationships { get; }
    DbSet<WatchedFilm> WatchedFilms { get; }
    DbSet<SavedFilm> SavedFilms { get; }
    DbSet<SavedList> SavedLists { get; }

    DbSet<FilmLike> FilmLikes { get; }
    DbSet<FilmTag> FilmTags { get; }
    DbSet<ReviewTag> ReviewTags { get; }
    DbSet<UserFavoriteFilm> UserFavoriteFilms { get; }


    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}