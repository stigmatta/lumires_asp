using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.Common;

[UsedImplicitly]
internal record CommonReviewResponse(
    Guid Id,
    int FilmId,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int RepliesCount,
    float? Rating,
    string? Title,
    string Text,
    int LikesCount,
    DateTime CreatedAt,
    bool IsLikedByMe,
    bool IsSpoilerFree
);