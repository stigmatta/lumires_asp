using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.GetThread;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<Response?> GetThreadByIdAsync(Query query, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        return await db.Threads
            .Where(x => x.Id == query.ThreadId)
            .Select(x => new Response(
                x.Id,
                x.UserId,
                x.User.Username,
                x.User.AvatarUrl,
                x.UserThreadComments.Count,
                x.Title,
                x.Image,
                x.Text,
                x.LikesCount,
                x.CreatedAt,
                currentUserId != Guid.Empty && x.Likes.Any(l => l.UserId == currentUserId),
                x.UserThreadComments.Select(c => new CommentItemResponse(
                    c.Id,
                    c.UserId,
                    c.Commentator.Username,
                    c.Commentator.AvatarUrl,
                    c.LikesCount,
                    currentUserId != Guid.Empty && c.Likes.Any(l => l.UserId == currentUserId),
                    c.Text,
                    c.CreatedAt,
                    c.TargetedUserId,
                    c.TargetedUser != null ? c.TargetedUser.Username : null
                )).ToList()
            ))
            .FirstOrDefaultAsync(ct);
    }
}