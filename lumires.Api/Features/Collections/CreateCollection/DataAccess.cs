using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Collections.CreateCollection;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, IStringLocalizer<SharedResource> localizer) : IDataAccess
{
    internal async Task<Result<Guid>> CreateCollectionAsync(Command command, Guid userId, CancellationToken ct)
    {
        var collection = new Collection(
            command.Title,
            userId,
            command.Description,
            command.IsPrivate
        );

        if (command.MovieIds is { Count: > 0 })
        {
            var movieIds = await db.Movies
                .Where(m => command.MovieIds.Contains(m.ExternalId))
                .Select(m => new { m.Id, m.ExternalId })
                .ToListAsync(ct);
            
            if (movieIds.Count != command.MovieIds.Count)
                return Result.Invalid(new ValidationError(localizer["ValidationError_SomeMovies_WereNotFound"]));

            foreach (var movie in movieIds)
                collection.AddMovie(movie.Id);
        }

        db.Collections.Add(collection);
        await db.SaveChangesAsync(ct);

        return collection.Id;
    }
}