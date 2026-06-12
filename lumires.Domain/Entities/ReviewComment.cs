using lumires.Domain.Base;

namespace lumires.Domain.Entities;

public sealed class ReviewComment : LikeableEntity<ReviewCommentLike>
{
    private ReviewComment()
    {
    }

    public ReviewComment(Guid commentatorId, Guid reviewId, string text, Guid? targetedUserId,
        bool isSpoilerFree = true)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;

        UserId = commentatorId;
        ReviewId = reviewId;
        TargetedUserId = targetedUserId;

        Text = text ?? throw new ArgumentNullException(nameof(text));
        IsSpoilerFree = isSpoilerFree;
    }


    public Guid Id { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }
    public User Commentator { get; private set; } = null!;
    public Guid UserId { get; private set; }

    public Review Review { get; private set; } = null!;
    public Guid ReviewId { get; private set; }
    public Guid? TargetedUserId { get; private set; }
    public User? TargetedUser { get; private set; }
    public string Text { get; private set; } = null!;
    public bool IsSpoilerFree { get; private set; }


    protected override Guid GetUserId(ReviewCommentLike like)
    {
        return like.UserId;
    }

    protected override ReviewCommentLike CreateLike(Guid userId)
    {
        return new ReviewCommentLike { ReviewCommentId = Id, UserId = userId, LikedAt = DateTimeOffset.UtcNow };
    }
    
    public void UpdateReviewComment(string? text, Guid? targetedUserId, bool? isSpoilerFree)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            Text = text;
        }

        if (targetedUserId.HasValue && targetedUserId != Guid.Empty)
        {
            TargetedUserId = targetedUserId;
        }

        if (isSpoilerFree.HasValue)
        {
            IsSpoilerFree = isSpoilerFree.Value;
        }
        UpdateTimestamp();
    }
    
    private void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    

    public void SetCommentator(User user)
    {
        Commentator = user;
    }

    public void SetTargetedUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        TargetedUser = user;
        TargetedUserId = user.Id;
    }
}