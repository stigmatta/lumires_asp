namespace lumires.Domain.Entities;

public sealed class ReviewComment
{
    private ReviewComment()
    {
    }

    public ReviewComment(User commentator, Review review, string text)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = CreatedAt;

        Commentator = commentator ?? throw new ArgumentNullException(nameof(commentator));
        UserId = commentator.Id;

        Review = review ?? throw new ArgumentNullException(nameof(review));
        ReviewId = review.Id;

        Text = text ?? throw new ArgumentNullException(nameof(text));
    }


    public Guid Id { get; }
    public DateOnly CreatedAt { get; }
    public DateOnly UpdatedAt { get; }
    public User Commentator { get; private set; }
    public Guid UserId { get; private set; }

    public Review Review { get; private set; }
    public Guid ReviewId { get; private set; }
    public Guid? TargetedUserId { get; private set; }
    public User? TargetedUser { get; private set; }
    public int LikesCount { get; private set; }
    public string Text { get; private set; }
}