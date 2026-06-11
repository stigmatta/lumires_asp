using lumires.Domain.Base;

namespace lumires.Domain.Entities;

public sealed class UserThread : LikeableEntity<UserThreadLike>
{
    private readonly List<UserThreadComment> _userThreadComments = [];

    private UserThread()
    {
    }

    public UserThread(Guid userId, string? title, string? image, string text, bool isSpoilerFree = true)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;

        UserId = userId;

        Title = title;
        Image = image;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        IsSpoilerFree = isSpoilerFree;
    }

    public Guid Id { get; }
    public User User { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public string? Title { get; private set; }
    public string? Image { get; private set; }
    public string Text { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsSpoilerFree { get; private set; }
    public bool IsEditorPick { get; private set; }


    public IReadOnlyCollection<UserThreadComment> UserThreadComments => _userThreadComments.AsReadOnly();

    protected override Guid GetUserId(UserThreadLike like)
    {
        return like.UserId;
    }

    protected override UserThreadLike CreateLike(Guid userId)
    {
        return new UserThreadLike { ThreadId = Id, UserId = userId, LikedAt = DateTimeOffset.UtcNow };
    }
    
    public void UpdateThread(string? title, string? text, string? image, bool? isSpoilerFree)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            Text = text;
        }

        if (!string.IsNullOrWhiteSpace(image))
        {
            Image = image;
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


    public void SetUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        User = user;
        UserId = user.Id;
    }

    public void SetEditorPick(bool editorPick)
    {
        IsEditorPick = editorPick;
    }
}