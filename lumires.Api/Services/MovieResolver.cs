using FastEndpoints;
using lumires.Core.Abstractions.Data;
using lumires.Core.Events.Movies;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Services;

internal class MovieResolver(IAppDbContext db) : IMovieResolver, IDataAccess
{
    public async Task<bool> EnsureMovieExistsAsync(int externalId, CancellationToken ct)
    {
        var exists = await db.Movies.AnyAsync(m => m.ExternalId == externalId, ct);
        if (exists) return true; // it was in the db

        await new MovieReferencedEvent { ExternalId = externalId }
            .PublishAsync(Mode.WaitForAll, ct);
        
        return false;
    }
}