using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Films.GetFilmStats;

[UsedImplicitly]
internal sealed record Query(int FilmId);

[UsedImplicitly]
internal sealed record Response(float VoteAverage, int TotalReviews, int ReviewsThisWeek, float? FriendsAverage);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFilmResolver filmResolver,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/films/{Slug}/{Id:int}/stats");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        // i think there is no need in this method returning just vote average, so it will be just empty object if film is not found

        // var lang = currentUserService.LangCulture;  
        //
        // await filmResolver.EnsureFilmExistsAsync(query.FilmId, lang, ct);

        var response = await db.GetFilmStats(query.FilmId, currentUserId, ct);

        await Send.OkAsync(response, ct);
    }
}