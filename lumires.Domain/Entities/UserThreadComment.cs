using lumires.Domain.Base;

namespace lumires.Domain.Entities;


public sealed class UserThreadComment : LikeableEntity<UserThreadCommentLike>
{
    private UserThreadComment()
    {
    }
    
    public UserThreadComment(Guid commentatorId, Guid threadId, string text, Guid? targetedUserId, bool isSpoilerFree = true)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);

        UserId = commentatorId;
        ThreadId = threadId;
        TargetedUserId = targetedUserId;

        Text = text ?? throw new ArgumentNullException(nameof(text));
    }


    public Guid Id { get; }
    public DateOnly CreatedAt { get; }
    public DateOnly? UpdatedAt { get; }
    public User Commentator { get; private set; } = null!;
    public Guid UserId { get; private set; }

    public UserThread Thread { get; private set; } = null!;
    public Guid ThreadId { get; private set; }
    public Guid? TargetedUserId { get; private set; }
    public User? TargetedUser { get; private set; }
    public string Text { get; private set; } = null!;
    public bool IsSpoilerFree { get; private set; }

    protected override Guid GetUserId(UserThreadCommentLike like)
    {
        return like.UserId;
    }

    protected override UserThreadCommentLike CreateLike(Guid userId)
    {
        return new UserThreadCommentLike { UserThreadCommentId = Id, UserId = userId };
    }
}