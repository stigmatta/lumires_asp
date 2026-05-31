using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Enums.Common;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace lumires.Api.Features.Threads.GetThreads;

internal enum ContentFilterEnum
{
    All,
    LongForm,
    Following,
    SpoilerFree
}

[UsedImplicitly]
internal sealed class Query
{
    public ContentFilterEnum? Category { get; init; } = ContentFilterEnum.All;
    public ContentOrderEnum? SortBy { get; init; } = ContentOrderEnum.MostRecent;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 5;
}

[UsedImplicitly]
internal sealed record ThreadItemResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int RepliesCount,
    string? Title,
    string Text,
    int LikesCount,
    DateOnly CreatedAt,
    bool IsLikedByMe,
    bool IsSpoilerFree,
    ThreadCommentItemResponse? Comment);

[UsedImplicitly]
internal sealed record ThreadCommentItemResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Text,
    int LikesCount
);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService)
    : Endpoint<Query, PagedResponse<ThreadItemResponse>>
{
    public override void Configure()
    {
        Get("/threads");
        Description(x => x.WithTags("Threads"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var userId = currentUserService.UserId;

        var response = await db.GetThreadsAsync(query, userId, ct);
        var count = await db.GetThreadsCountAsync(query, ct);

        var paged = new PagedResponse<ThreadItemResponse>(
            response,
            count,
            query.Page,
            query.PageSize
        );

        await Send.OkAsync(paged, ct);
    }
}