using lumires.Domain.Base;

namespace lumires.Domain.Entities;

public sealed class ReviewComment : LikeableEntity<ReviewCommentLike>
{
    private ReviewComment()
    {
    }

    public ReviewComment(Guid commentatorId, Guid reviewId, string text, Guid? targetedUserId)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = CreatedAt;

        UserId = commentatorId;
        ReviewId = reviewId;
        TargetedUserId = targetedUserId;

        Text = text ?? throw new ArgumentNullException(nameof(text));
    }


    public Guid Id { get; }
    public DateOnly CreatedAt { get; }
    public DateOnly UpdatedAt { get; }
    public User Commentator { get; private set; } = null!;
    public Guid UserId { get; private set; }

    public Review Review { get; private set; } = null!;
    public Guid ReviewId { get; private set; }
    public Guid? TargetedUserId { get; private set; }
    public User? TargetedUser { get; private set; }
    public string Text { get; private set; } = null!;

    protected override Guid GetUserId(ReviewCommentLike like)
    {
        return like.UserId;
    }

    protected override ReviewCommentLike CreateLike(Guid userId)
    {
        return new ReviewCommentLike { ReviewCommentId = Id, UserId = userId };
    }
}