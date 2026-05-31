using lumires.Domain.Base;

namespace lumires.Domain.Entities;

public sealed class UserThread: LikeableEntity<UserThreadLike>
{
    private readonly List<UserThreadComment> _userThreadComments = [];
    
    private UserThread() {}
    
    public UserThread(Guid userId, string? title, string text, bool isSpoilerFree = true)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);

        UserId = userId;

        Title = title;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        IsSpoilerFree = isSpoilerFree;
    }
    
    public Guid Id { get; }
    public User User { get; private set; } = null!;
    public Guid UserId { get; }
    public string? Title { get; private set; }
    public string Text { get; private set; } = null!;
    public DateOnly CreatedAt { get; private set; }
    public DateOnly? UpdatedAt { get; private set; }
    public bool IsSpoilerFree { get; private set; }
    
    
    public IReadOnlyCollection<UserThreadComment> UserThreadComments => _userThreadComments.AsReadOnly();

    protected override Guid GetUserId(UserThreadLike like)
    {
        return like.UserId;
    }

    protected override UserThreadLike CreateLike(Guid userId)
    {
        return new UserThreadLike { ThreadId = Id, UserId = userId };
    }
}