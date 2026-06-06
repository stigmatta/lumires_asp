using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.LikeFilm;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{
    internal async Task<Result<Response>> ToggleLikeAsync(Guid filmId, CancellationToken ct)
    {
        var film = await db.Films
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == filmId, ct);

        if (film is null) return Result.NotFound();

        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var isLiked = film.ToggleLike(currentUserId);

        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, film.LikesCount);
        return Result.Success(response);
    }
}