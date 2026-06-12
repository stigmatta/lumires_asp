using lumires.Domain.Base;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class Review : LikeableEntity<ReviewLike>
{
    private readonly List<ReviewComment> _reviewComments = [];
    private readonly List<ReviewTag> _tags = [];

    private Review()
    {
    }

    public Review(Guid userId, Guid filmId, string? title, string text, float? rating,
        bool isSpoilerFree = true)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;

        UserId = userId;
        FilmId = filmId;

        Title = title;
        Text = text ?? throw new ArgumentNullException(nameof(text));

        if (rating is not null)
        {
            if (rating is < 0 or > 5)
                throw new DomainException("Rating must be between 0 and 5.", nameof(rating));

            var remainder = rating % 0.5f;
            if (remainder != 0)
                throw new DomainException("Rating must be a multiple of 0.5 (e.g. 1, 1.5, 2, 2.5).", nameof(rating));
        }

        Rating = rating;
        IsSpoilerFree = isSpoilerFree;
    }


    public Guid Id { get; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public User Reviewer { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Film Film { get; private set; } = null!;
    public Guid FilmId { get; private set; }
    public string? Title { get; private set; }
    public string Text { get; private set; } = null!;
    public float? Rating { get; private set; }
    public bool IsSpoilerFree { get; private set; }
    public bool IsEditorPick { get; private set; }

    public IReadOnlyCollection<ReviewComment> ReviewComments => _reviewComments.AsReadOnly();
    public IReadOnlyCollection<ReviewTag> Tags => _tags.AsReadOnly();


    protected override Guid GetUserId(ReviewLike like)
    {
        return like.UserId;
    }

    protected override ReviewLike CreateLike(Guid userId)
    {
        return new ReviewLike { ReviewId = Id, UserId = userId, LikedAt = DateTimeOffset.UtcNow };
    }

    public void SetReviewer(User reviewer)
    {
        Reviewer = reviewer ?? throw new ArgumentNullException(nameof(reviewer));
        UserId = reviewer.Id;
    }

    public void SetFilm(Film film)
    {
        Film = film ?? throw new ArgumentNullException(nameof(film));
        FilmId = film.Id;
    }

    public void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            ArgumentNullException.ThrowIfNull(text);

        Text = text;
    }

    public void AddComment(ReviewComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);
        _reviewComments.Add(comment);
    }

    public void SetCreatedAt(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

    public void UpdateReview(string? title, string? text, float? rating, bool? isSpoilerFree)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            Text = text;
        }

        if (rating.HasValue)
        {
            Rating = rating;
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

    public void SetEditorPick(bool editorPick)
    {
        IsEditorPick = editorPick;
    }

    public void AddTag(Tag tag)
    {
        if (_tags.Any(t => t.TagId == tag.Id)) return;
        _tags.Add(new ReviewTag { ReviewId = Id, TagId = tag.Id });
    }
}