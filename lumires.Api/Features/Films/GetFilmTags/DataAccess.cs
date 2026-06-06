using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetFilmTags;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Response>> GetFilmTags(int filmId, CancellationToken ct)
    {
        var tags = await db.FilmTags
            .Where(t => t.Film.ExternalId == filmId)
            .Select(t => new TagItem(
                t.Tag.Id,
                t.Tag.Name,
                t.Tag.Slug))
            .ToListAsync(ct);

        return new Response(tags);
    }
}