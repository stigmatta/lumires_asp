using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetFilmRatingBreakdown;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Dictionary<float, int>> GetFilmRatingsDictionaryAsync(int filmId, CancellationToken ct)
    {
        return await db.UserFilmRatings
            .Where(x => x.Film.ExternalId == filmId)
            .GroupBy(x => x.Rating)
            .OrderBy(g => g.Key)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count, ct);
    }
}