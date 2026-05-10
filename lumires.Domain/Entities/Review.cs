namespace lumires.Domain.Entities;

public sealed class Review
{
    private readonly List<ReviewComment> _reviewComments = [];
    private readonly List<ReviewLike> _likes = [];


    private Review()
    {
    }

    public Review(Guid userId, Guid movieId, string? title, string text, decimal? rating, 
        bool isSpoilerFree)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = CreatedAt;

        UserId = userId;
        MovieId = movieId;

        Title = title;
        Text = text ?? throw new ArgumentNullException(nameof(text));

        if (rating is not null)
        {
            if (rating is < 0 or > 5)
                throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 5.");

            var remainder = rating % 0.5m;
            if (remainder != 0)
                throw new ArgumentException("Rating must be a multiple of 0.5 (e.g. 1, 1.5, 2, 2.5).", nameof(rating));
        }

        Rating = rating;
        IsSpoilerFree = isSpoilerFree;
    }


    public Guid Id { get; }
    public DateOnly CreatedAt { get; }
    public DateOnly UpdatedAt { get; }
    public User Reviewer { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Movie Movie { get; private set; } = null!;
    public Guid MovieId { get; private set; }
    public int LikesCount { get; private set; }
    public string? Title { get; private set; }
    public string Text { get; private set; }
    public decimal? Rating { get; private set; }
    public bool IsSpoilerFree { get; private set; }
    public IReadOnlyCollection<ReviewComment> ReviewComments => _reviewComments.AsReadOnly();
    public IReadOnlyCollection<ReviewLike> Likes => _likes.AsReadOnly();

    public bool ToggleLike(Guid userId)
    {
        var existing = _likes.FirstOrDefault(l => l.UserId == userId);
        if (existing is not null)
        {
            _likes.Remove(existing);
            LikesCount--;
            return false;
        }

        _likes.Add(new ReviewLike { ReviewId = Id, UserId = userId });
        LikesCount++;
        return true;
    }
}