using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetUserProfile;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<Result<Response>> GetUserProfile(string username, CancellationToken ct)
    {
        var currentUser = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUserService.UserId, ct);

        var user = await db.Users
            .Where(u => u.Username == username)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.DisplayName,
                Pronouns = u.Pronouns.ToString(),
                u.Location,
                u.Tagline,
                u.Biography,
                Followers = u.IncomingRelationships.Count(r =>
                    r.Type == UserRelationshipType.Follow &&
                    r.Status == UserRelationshipStatus.Accepted),
                Followings = u.OutgoingRelationships.Count(r =>
                    r.Type == UserRelationshipType.Follow &&
                    r.Status == UserRelationshipStatus.Accepted),
                Friends = u.OutgoingRelationships.Count(r =>
                    r.Type == UserRelationshipType.Follow &&
                    r.Status == UserRelationshipStatus.Accepted &&
                    u.IncomingRelationships.Any(i =>
                        i.Type == UserRelationshipType.Follow &&
                        i.Status == UserRelationshipStatus.Accepted &&
                        i.SourceUserId == r.TargetUserId)),
                IsMe = currentUser != null && currentUser.Id == u.Id,
                IsBlocked = currentUser != null &&
                            currentUser.OutgoingRelationships.Any(r =>
                                r.TargetUserId == u.Id &&
                                r.Type == UserRelationshipType.Block),
                IsFollowed = currentUser != null &&
                             currentUser.OutgoingRelationships.Any(r =>
                                 r.TargetUserId == u.Id &&
                                 r.Type == UserRelationshipType.Follow),
                ProfileVisibilty = u.UserSettings.ProfileVisibility
            })
            .FirstOrDefaultAsync(ct);

        if (user is null)
            return Result.NotFound();

        if (user.ProfileVisibilty == ProfileVisibility.Everyone || user.IsMe)
            return new Response(user.Username, user.DisplayName, user.Pronouns, user.Location,
                user.Tagline, user.Biography, user.Followers, user.Followings, user.Friends, user.IsMe, user.IsFollowed,
                user.IsBlocked);

        switch (user.ProfileVisibilty)
        {
            case ProfileVisibility.OnlyMe when !user.IsMe:
                return Result.Forbidden();
            case ProfileVisibility.FollowersOnly when !user.IsMe:
            {
                if (currentUser is null)
                    return Result.Forbidden();
                
                var isFollower = await db.Relationships.AnyAsync(r =>
                        r.SourceUserId == currentUser.Id &&
                        r.TargetUserId == user.Id &&
                        r.Type == UserRelationshipType.Follow &&
                        r.Status == UserRelationshipStatus.Accepted,
                    ct);

                if (!isFollower)
                    return Result.Forbidden();
                break;
            }
            case ProfileVisibility.Everyone:
            default:
                break;
        }

        return new Response(user.Username, user.DisplayName, user.Pronouns, user.Location,
            user.Tagline, user.Biography, user.Followers, user.Followings, user.Friends, user.IsMe, user.IsFollowed,
            user.IsFollowed);
    }
}