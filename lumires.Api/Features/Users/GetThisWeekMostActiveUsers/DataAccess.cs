using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetThisWeekMostActiveUsers;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response> GetMostActiveUsersAsync(
        CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var items = await db.Users
            .AsNoTracking()
            .OrderByDescending(u =>
                u.Reviews.Count(r => r.CreatedAt >= weekAgo) +
                u.FilmsLists.Count(l => l.CreatedAt >= weekAgo))
            .Select(u => new MemberItem(
                u.Id,
                u.Username,
                u.Reviews.Count,
                u.FilmsLists.Count))
            .ToArrayAsync(ct);

        return new Response(items);
    }
}