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
        var currentUserId = currentUserService.UserId;

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
                u.AvatarUrl,
                u.Biography,
                u.IncomingRelationships,
                u.OutgoingRelationships,
                IsMe = currentUserId != Guid.Empty && currentUserId == u.Id,
                ProfileVisibilty = u.UserSettings.ProfileVisibility,
                ReviewsWritten = u.Reviews.Count,
                ThreadsWritten = u.UserThreads.Count,
                ListsCreated = u.FilmsLists
                    .Count(x => !x.IsPrivate || x.UserId == currentUserId)
            })
            .FirstOrDefaultAsync(ct);

        if (user is null)
            return Result.NotFound();

        var followers = user.IncomingRelationships.Count(r =>
            r is { Type: UserRelationshipType.Follow, Status: UserRelationshipStatus.Accepted });

        var followings = user.OutgoingRelationships.Count(r =>
            r is { Type: UserRelationshipType.Follow, Status: UserRelationshipStatus.Accepted });

        var friends = user.OutgoingRelationships.Count(r =>
            r is { Type: UserRelationshipType.Follow, Status: UserRelationshipStatus.Accepted } &&
            user.IncomingRelationships.Any(i =>
                i is { Type: UserRelationshipType.Follow, Status: UserRelationshipStatus.Accepted } &&
                i.SourceUserId == r.TargetUserId));

        var outgoing = user.IncomingRelationships
            .Where(r => r.SourceUserId == currentUserId)
            .Select(x => new Relationship(x.Type, x.Status))
            .FirstOrDefault();

        var incoming = user.OutgoingRelationships
            .Where(r => r.TargetUserId == currentUserId)
            .Select(x => new Relationship(x.Type, x.Status))
            .FirstOrDefault();

        if (incoming is not null && incoming.Type == UserRelationshipType.Block) return Result.Forbidden();

        if (user.ProfileVisibilty == ProfileVisibility.Everyone || user.IsMe)
            return new Response(user.Id, user.Username, user.DisplayName, user.Pronouns, user.Location,
                user.Tagline, user.AvatarUrl, user.Biography, followers, followings, friends, user.IsMe, incoming,
                outgoing, user.ReviewsWritten, user.ThreadsWritten, user.ListsCreated);

        switch (user.ProfileVisibilty)
        {
            case ProfileVisibility.OnlyMe when !user.IsMe:
                return Result.Forbidden();
            case ProfileVisibility.FollowersOnly when !user.IsMe:
            {
                var isFollower = await db.Relationships.AnyAsync(r =>
                        r.SourceUserId == currentUserId &&
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

        return new Response(user.Id, user.Username, user.DisplayName, user.Pronouns, user.Location,
            user.Tagline, user.AvatarUrl, user.Biography, followers, followings, friends, user.IsMe, incoming, outgoing,
            user.ReviewsWritten, user.ThreadsWritten, user.ListsCreated);
    }
}