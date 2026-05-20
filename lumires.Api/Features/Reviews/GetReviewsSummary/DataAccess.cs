using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviewsSummary;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<int> GetReviewsFromDaySpan(int days, CancellationToken ct)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var daySpan = now.AddDays(-days);

        return await db.Reviews
            .AsNoTracking()
            .CountAsync(r => r.CreatedAt >= daySpan, ct);
    }
}