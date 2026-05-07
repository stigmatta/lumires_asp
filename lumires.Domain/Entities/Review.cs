using lumires.Domain.Enums;

namespace lumires.Domain.Entities;

public sealed class Review
{
    private readonly List<ReviewComment> _reviewComments = [];

    private Review()
    {
    }

    public Review(User reviewer, Movie movie, ReviewType type, string? title, string text, decimal? rating)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = CreatedAt;

        ReviewType = type;

        Reviewer = reviewer ?? throw new ArgumentNullException(nameof(reviewer));
        UserId = reviewer.Id;

        Movie = movie ?? throw new ArgumentNullException(nameof(movie));
        MovieId = movie.Id;

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
    }


    public Guid Id { get; }
    public DateOnly CreatedAt { get; }
    public DateOnly UpdatedAt { get; }
    public ReviewType ReviewType { get; }
    public User Reviewer { get; private set; }
    public Guid UserId { get; private set; }
    public Movie Movie { get; private set; }
    public Guid MovieId { get; private set; }
    public int LikesCount { get; private set; }
    public string? Title { get; private set; }
    public string Text { get; private set; }
    public decimal? Rating { get; private set; }
    public IReadOnlyCollection<ReviewComment> ReviewComments => _reviewComments.AsReadOnly();
}