using lumires.Domain.Base;

namespace lumires.Domain.Entities;

public sealed class Review : LikeableEntity<ReviewLike>
{
    private readonly List<ReviewComment> _reviewComments = [];

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
    public string? Title { get; private set; }
    public string Text { get; private set; }
    public decimal? Rating { get; private set; }
    public bool IsSpoilerFree { get; private set; }
    public IReadOnlyCollection<ReviewComment> ReviewComments => _reviewComments.AsReadOnly();

    protected override Guid GetUserId(ReviewLike like)
    {
        return like.UserId;
    }

    protected override ReviewLike CreateLike(Guid userId)
    {
        return new ReviewLike { ReviewId = Id, UserId = userId };
    }
}