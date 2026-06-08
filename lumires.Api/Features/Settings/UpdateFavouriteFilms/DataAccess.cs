using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Settings.UpdateFavouriteFilms;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> UpdateFavoriteFilms(Command command, Guid userId, CancellationToken ct)
    {
        var currentUser = await db.Users
            .Include(u => u.UserSettings)
            .ThenInclude(s => s.FavoriteFilms)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (currentUser is null) return Result.Forbidden();

        var filmIds = command.FavouriteFilms.Select(f => f.ExternalId).ToList();

        var films = await db.Films
            .Where(f => filmIds.Contains(f.ExternalId))
            .ToListAsync(ct);

        var filmDict = films.ToDictionary(f => f.ExternalId, f => f.Id);

        db.UserFavoriteFilms.RemoveRange(currentUser.UserSettings.FavoriteFilms);

        var newFavorites = command.FavouriteFilms
            .Where(f => filmDict.ContainsKey(f.ExternalId))
            .Select(f => new UserFavoriteFilm(
                currentUser.UserSettings.Id,
                filmDict[f.ExternalId],
                f.Order
            ))
            .ToList();

        db.UserFavoriteFilms.AddRange(newFavorites);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}