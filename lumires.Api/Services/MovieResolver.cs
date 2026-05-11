using FastEndpoints;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Movies;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Services;

internal class MovieResolver(IAppDbContext db) : IMovieResolver, IDataAccess
{
    public async Task<bool> EnsureMovieExistsAsync(
        int externalId,
        string language,
        CancellationToken ct)
    {
        var exists = await db.Movies
            .AnyAsync(m => m.ExternalId == externalId, ct);

        if (exists)
            return true;

        await new MovieReferencedEvent
            {
                ExternalId = externalId,
                Language = language
            }
            .PublishAsync(Mode.WaitForAll, ct);

        await new MovieEnrichmentEvent
        {
            ExternalId = externalId,
            SkipLanguage = language
        }.PublishAsync(Mode.WaitForNone, ct);

        return false;
    }
}