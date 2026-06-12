using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Messaging;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.LikeReviewComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;
    
    internal async Task<Result<Response>> ToggleLikeAsync(Guid reviewCommentId, CancellationToken ct)
    {
        var reviewComment = await db.ReviewComments
            .Include(r => r.Likes)
            .Include(r => r.Review)
            .ThenInclude(r => r.Film)
            .Include(r => r.Commentator)
            .ThenInclude(r => r.UserSettings)
            .FirstOrDefaultAsync(r => r.Id == reviewCommentId, ct);

        if (reviewComment is null) return Result.NotFound();

        var currentUserId = currentUserService.UserId;
        var currentUser = await db.Users
            .Where(x => x.Id == currentUserService.UserId)
            .Select(x => new
            {
                x.Username,
                x.AvatarUrl
            }).FirstAsync(ct);

        var lang = currentUserService.LangCulture;
        var isLiked = reviewComment.ToggleLike(currentUserId);

        if (isLiked && reviewComment.Commentator.UserSettings.Notifications.LikesOnContent)
        {
            var message = new NotificationMessage(NotificationType.LikedReviewComment, currentUserId.ToString(),
                currentUser.Username,
                currentUser.AvatarUrl,
                reviewComment.ReviewId.ToString(),
                reviewComment.Review.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(f => f.Title)
                    .First(),
                DateTime.UtcNow);

            notificationService.SendToUser(reviewComment.UserId, message);
        }


        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, reviewComment.LikesCount);
        return Result.Success(response);
    }
}