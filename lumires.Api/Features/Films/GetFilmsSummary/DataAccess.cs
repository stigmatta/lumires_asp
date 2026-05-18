using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetFilmsSummary;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<int> GetTotalGenresCount(CancellationToken ct)
    {
        return await db.Genres
            .AsNoTracking()
            .CountAsync(ct);
    }
}