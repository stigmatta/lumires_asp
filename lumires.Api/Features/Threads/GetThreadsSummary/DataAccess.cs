using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.GetThreadsSummary;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<int> GetThreadsFromDaySpan(int days, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var daySpan = now.AddDays(-days);

        return await db.Threads
            .AsNoTracking()
            .CountAsync(r => r.CreatedAt >= daySpan, ct);
    }
}