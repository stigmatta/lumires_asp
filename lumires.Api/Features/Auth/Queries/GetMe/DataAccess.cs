using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Auth.Queries.GetMe;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response?> UserOrNullAsync(Guid userId, CancellationToken ct)
    {
        return await db.Users
            .Where(x => x.Id == userId)
            .Select(x => new Response(x.Id, x.Email, x.Username, x.AvatarUrl))
            .FirstOrDefaultAsync(ct);
    }
}