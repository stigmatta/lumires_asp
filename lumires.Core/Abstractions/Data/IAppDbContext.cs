using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.Abstractions.Data;

public interface IAppDbContext: IDisposable, IAsyncDisposable
{
    DbSet<Movie> Movies { get; }
    DbSet<MovieLocalization> MovieLocalizations { get; }

    DbSet<UserNotification> UserNotifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}