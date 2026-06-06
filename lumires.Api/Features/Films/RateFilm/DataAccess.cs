using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.RateFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> RateFilmAsync(Command command, Guid userId, CancellationToken ct)
    {
        var filmId = await db.Films
            .Where(m => m.ExternalId == command.FilmId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);

        if (filmId == Guid.Empty) return Result.NotFound();


        var userRating = await db.UserFilmRatings
            .Where(fr => fr.FilmId == filmId && fr.UserId == userId)
            .FirstOrDefaultAsync(ct);

        UserFilmRating newRating = null!;

        if (userRating is not null)
        {
            userRating.UpdateRating(command.Rating);
        }
        else
        {
            newRating = new UserFilmRating(userId, filmId, command.Rating);
            db.UserFilmRatings.Add(newRating);
        }

        await db.SaveChangesAsync(ct);

        return userRating?.FilmId ?? newRating.FilmId;
    }
}