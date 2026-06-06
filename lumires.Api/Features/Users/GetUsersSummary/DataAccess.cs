using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetUsersSummary;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response> GetUsersSummary(CancellationToken ct)
    {
        var recentlyOnline = DateTimeOffset.UtcNow.AddMinutes(-15);

        var totalMembers = await db.Users.CountAsync(ct);
        var onlineNow = await db.Users.CountAsync(u => u.LastActiveAt >= recentlyOnline, ct);

        return new Response(totalMembers, onlineNow);
    }
}