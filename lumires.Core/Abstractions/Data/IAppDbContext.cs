using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Abstractions.Data;

public interface IAppDbContext
{
    DbSet<Movie> Movies { get; }
    DbSet<MovieLocalization> MovieLocalizations { get; }

    DbSet<UserNotification> UserNotifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}