using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Messaging;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.LikeReview;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;
    
    internal async Task<Result<Response>> ToggleLikeAsync(Guid reviewId, CancellationToken ct)
    {
        var review = await db.Reviews
            .Include(r => r.Likes)
            .Include(r => r.Film)
            .ThenInclude(f => f.Localizations)
            .Include(r => r.Reviewer)
            .ThenInclude(r => r.UserSettings)
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        var currentUser = await db.Users
            .Where(x => x.Id == currentUserService.UserId)
            .Select(x => new
            {
                x.Username,
                x.AvatarUrl
            }).FirstAsync(ct);

        if (review is null) return Result.NotFound();
        var lang = currentUserService.LangCulture;

        var isLiked = review.ToggleLike(currentUserService.UserId);

        if (isLiked && review.Reviewer.UserSettings.Notifications.LikesOnContent)
        {
            var message = new NotificationMessage(
                NotificationType.LikedReview,
                currentUserService.UserId.ToString(),
                currentUser.Username,
                currentUser.AvatarUrl,
                review.Id.ToString(),
                review.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(f => f.Title)
                    .First(),
                DateTime.UtcNow);

            notificationService.SendToUser(review.UserId, message);
        }

        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, review.LikesCount);
        return Result.Success(response);
    }
}