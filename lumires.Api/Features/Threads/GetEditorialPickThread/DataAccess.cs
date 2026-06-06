using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.GetEditorialPickThread;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Response?> GetEditorialThreadAsync(Guid userId, CancellationToken ct)
    {
        return await db.Threads
            .AsNoTracking()
            .Where(x => x.IsEditorPick)
            .OrderByDescending(x => x.LikesCount)
            .Select(x => new Response(
                x.Id,
                x.Title,
                x.Text,
                x.UserId,
                x.User.Username,
                x.CreatedAt,
                x.UserThreadComments.Count,
                x.LikesCount,
                x.UserThreadComments
                    .Select(c => new ThreadCommentPreview(
                        c.Id,
                        c.UserId,
                        c.Commentator.Username,
                        c.Commentator.AvatarUrl,
                        c.Text,
                        c.LikesCount,
                        x.Likes.Any(l => l.UserId == userId),
                        c.CreatedAt))
                    .ToList()))
            .FirstOrDefaultAsync(ct);
    }
}