using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.FilmsLists.CreateFilmsList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, IStringLocalizer<SharedResource> localizer) : IDataAccess
{
    internal async Task<Result<Guid>> CreateFilmsListAsync(Command command, Guid userId, CancellationToken ct)
    {
        var collection = new FilmsList(
            command.Title,
            userId,
            command.Description,
            command.IsPrivate
        );

        if (command.FilmIds is { Count: > 0 })
        {
            var movieIds = await db.Films
                .Where(m => command.FilmIds.Contains(m.ExternalId))
                .Select(m => new { m.Id, m.ExternalId })
                .ToListAsync(ct);

            if (movieIds.Count != command.FilmIds.Count)
                return Result.Invalid(new ValidationError(localizer["ValidationError_SomeFilms_WereNotFound"]));

            foreach (var movie in movieIds)
                collection.AddFilm(movie.Id);
        }

        db.FilmsLists.Add(collection);
        await db.SaveChangesAsync(ct);

        return collection.Id;
    }
}