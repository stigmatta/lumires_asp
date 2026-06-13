using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetUserProfileSummary;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response?> GetUserSummary(string username,  CancellationToken ct)
    {
        return await db.Users
            .Where(u => u.Username == username)
            .Select(u => new Response(
                    u.FilmRatings.Count,
                    u.FilmsLists.Count,
                    u.Reviews.Count,
                    u.WatchlistFilms.Count,
                    db.Films.Count(f => f.Likes.Any(l => l.UserId == u.Id))
                    + db.FilmsLists.Count(f => f.Likes.Any(l => l.UserId == u.Id))
                    + db.Reviews.Count(r => r.Likes.Any(l => l.UserId == u.Id)),
                    u.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}