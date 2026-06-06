using System.Security.Claims;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Middleware;

public sealed class LastActiveMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAppDbContext db, IFusionCache cache)
    {
        await next(context);

        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(cache);

        if (!context.User.Identity?.IsAuthenticated ?? true) return;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var guid)) return;

        var cacheKey = $"last_active:{guid}";

        var cached = await cache.TryGetAsync<bool>(cacheKey);
        if (cached.HasValue) return;

        await cache.SetAsync(cacheKey, true, TimeSpan.FromMinutes(15));

        await db.Users
            .Where(u => u.Id == guid)
            .ExecuteUpdateAsync(u => u.SetProperty(
                x => x.LastActiveAt,
                DateTimeOffset.UtcNow));
    }
}