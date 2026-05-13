using FastEndpoints;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Films;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Services;

internal class FilmResolver(IAppDbContext db) : IFilmResolver, IResolver
{
    public async Task<bool> EnsureFilmExistsAsync(
        int externalId,
        string language,
        CancellationToken ct)
    {
        var exists = await db.Films
            .AnyAsync(m => m.ExternalId == externalId, ct);

        if (exists)
            return true;

        await new FilmReferencedEvent
            {
                ExternalIds = [externalId],
                Language = language
            }
            .PublishAsync(Mode.WaitForAll, ct);

        await new FilmEnrichmentEvent
        {
            ExternalIds = [externalId],
            SkipLanguage = language
        }.PublishAsync(Mode.WaitForNone, ct);

        return false;
    }
}