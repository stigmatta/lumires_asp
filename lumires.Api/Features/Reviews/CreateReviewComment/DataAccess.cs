using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Messaging;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Reviews.CreateReviewComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    INotificationService notificationService,
    ICurrentUserService currentUserService,
    IStringLocalizer<SharedResource> localizer) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;
 
    internal async Task<Result<Response>> CreateReviewCommentAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;
        var currentUser = await db.Users
            .Where(u => u.Id == currentUserId)
            .Select(u => new
            {
                u.Username,
                u.AvatarUrl
            }).FirstOrDefaultAsync(ct);
        
        var review = await db.Reviews
            .Where(m => m.Id == command.ReviewId)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                RepliesAllowed = x.Reviewer.UserSettings.Notifications.RepliesAndMentions,
                FilmTitle = x.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(f => f.Title)
                    .First()
            })
            .FirstOrDefaultAsync(ct);

        if (review is null) return Result.NotFound();
        
        User? targetedUser = null;
        if (command.TargetedUserId.HasValue)
        {
            targetedUser = await db.Users
                .Include(u => u.UserSettings)
                .FirstOrDefaultAsync(u => u.Id == command.TargetedUserId.Value, ct);


            if (targetedUser is null)
                return Result.Invalid(new ValidationError("TargetedUserId", localizer["ValidationError_UserId_Invalid"]));
        }

        var reviewComment = new ReviewComment(
            review.UserId,
            command.ReviewId,
            command.Text,
            command.TargetedUserId,
            command.IsSpoilerFree);

        db.ReviewComments.Add(reviewComment);

        if (review.RepliesAllowed)
        {
            var message = new NotificationMessage(
                NotificationType.ReviewReplied,
                currentUserId.ToString(),
                currentUser!.Username,
                currentUser.AvatarUrl,
                review.Id.ToString(),
                review.FilmTitle,
                DateTime.UtcNow);

            if (targetedUser is not null && targetedUser.UserSettings.Notifications.RepliesAndMentions)
            {
                notificationService.SendToUsers(review.UserId, targetedUser.Id, message);
            }
            else
            {
                notificationService.SendToUser(review.UserId, message);
            }
        }

        await db.SaveChangesAsync(ct);

        return new Response(
            reviewComment.Id,
            reviewComment.Text,
            reviewComment.CreatedAt,
            reviewComment.IsSpoilerFree);
    }

}