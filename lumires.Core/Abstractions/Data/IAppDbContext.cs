using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace lumires.Core.Abstractions.Data;

public interface IAppDbContext : IDisposable, IAsyncDisposable
{
    DbSet<Movie> Movies { get; }
    DbSet<MovieLocalization> MovieLocalizations { get; }

    DbSet<UserNotification> UserNotifications { get; }
    DbSet<Collection> Collections { get; }
    DbSet<User> Users { get; }
    DbSet<Genre> Genres { get; }
    DbSet<GenreLocalization> GenreLocalizations { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}