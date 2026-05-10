namespace lumires.Domain.Base;

public abstract class LikeableEntity<TLike> where TLike : class
{
    private readonly List<TLike> _likes = [];
    public IReadOnlyCollection<TLike> Likes => _likes.AsReadOnly();
    public int LikesCount { get; private set; }

    protected abstract Guid GetUserId(TLike like);
    protected abstract TLike CreateLike(Guid userId);

    public bool ToggleLike(Guid userId)
    {
        var existing = _likes.FirstOrDefault(l => GetUserId(l) == userId);
        if (existing is not null)
        {
            _likes.Remove(existing);
            LikesCount--;
            return false;
        }

        _likes.Add(CreateLike(userId));
        LikesCount++;
        return true;
    }
}