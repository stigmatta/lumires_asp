using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsSummary;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<int> GetListsFromSpan(int days, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var daySpan = now.AddDays(-days);

        return await db.FilmsLists
            .AsNoTracking()
            .CountAsync(r => r.CreatedAt >= daySpan, ct);
    }
    
    public async Task<int> GetListsTotalCount(CancellationToken ct)
    {
        return await db.FilmsLists
            .AsNoTracking()
            .CountAsync(ct);
    }
}