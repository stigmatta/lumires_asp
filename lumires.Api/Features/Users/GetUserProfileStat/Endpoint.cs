using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Users.GetUserProfileStat;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record Response(
    string[] MostWatchedDirectors,
    string[] MostWatchedDecades,
    string[] MostWatchedGenres,
    Dictionary<float, int> AverageRatings);


internal sealed class Endpoint(
    DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/users/{username}/stats");
        Description(x => x.WithTags("Users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var response = await db.GetUserProfile(query.Username, lang, ct);

        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        
        await Send.OkAsync(response, ct);
    }

}