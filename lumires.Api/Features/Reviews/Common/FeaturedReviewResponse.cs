using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.Common;

[UsedImplicitly]
internal record FeaturedReviewResponse(
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
    int MinutesRead);