using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Api.Features.Reviews.GetPopularReviewsInDaySpan;

[UsedImplicitly]
internal sealed record Query(int DaySpan);

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<PopularReviewItem> Items);

[UsedImplicitly]
internal sealed record PopularReviewItem(
    Guid Id,
    int FilmId,
    string FilmTitle,
    string FilmSlug,
    string? PosterPath,
    int? ReleaseYear,
    string[] Genres,
    int Runtime,
    Guid DirectorId,
    string DirectorName,
    string? Title,
    string Text,
    Guid UserId,
    string Username,
    DateTime CreatedAt,
    float? Rating,
    int LikesCount,
    int RepliesCount,
    bool IsLikedByMe,
    bool IsEditorPick,
    int MinutesRead
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFusionCache cache,
    DataAccess dataAccess)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/reviews/popular/{DaySpan}");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var cacheKey = CacheKeys.PopularReviewsBySpan(query.DaySpan);

        Response = await cache.GetOrSetAsync<Response>(
            cacheKey,
            async (_, token) =>
            {
                var result = await dataAccess.GetPopularReviewsBySpan(query.DaySpan, lang, token);
                return result ?? new Response([]);
            },
            options => options.SetDuration(CacheDuration.Long).SetFailSafe(true),
            ct
        );
    }
}