using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetSimilarFilms;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<int[]?> GetExistingFilms(int[] ids, CancellationToken ct)
    {
        var items = await db.Films
            .AsNoTracking()
            .Where(x => !ids.Contains(x.ExternalId))
            .Select(x => x.ExternalId)
            .ToArrayAsync(ct);

        return items;
    }
}