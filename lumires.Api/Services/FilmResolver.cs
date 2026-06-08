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
        var film = await db.Films
            .Include(p => p.Localizations)
            .FirstOrDefaultAsync(m => m.ExternalId == externalId, ct);

        if (film is not null && film.Localizations.Any(d => d.LanguageCode == language))
            return true;

        await new FilmReferencedEvent
        {
            ExternalIds = [externalId],
            Language = language
        }.PublishAsync(Mode.WaitForAll, ct);

        return false;
    }
}