using System.Text.RegularExpressions;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed partial class User
{
    private readonly List<FilmsList> _filmsLists = [];
    private readonly List<ReviewComment> _reviewComments = [];
    private readonly List<Review> _reviews = [];


    private User()
    {
    }

    public User(Guid id, string? username, string email)
    {
        if (id == Guid.Empty) throw new UserValidationException("UserId is invalid");

        if (!IsEmailValid(email))
            throw new UserValidationException("Email is not valid");

        if (string.IsNullOrWhiteSpace(username)) username = email.Split('@')[0];

        if (!IsUsernameValid(username))
            throw new UserValidationException("Username contains invalid characters or starts incorrectly.");

        Id = id;
        Username = username;
        Email = email;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<FilmsList> FilmsLists => _filmsLists.AsReadOnly();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    public IReadOnlyCollection<ReviewComment> ReviewComments => _reviewComments.AsReadOnly();


    public void SetAvatarUrl(string avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarUrl);
        AvatarUrl = avatarUrl;
    }

    public static bool IsUsernameValid(string username)
    {
        return UsernameRegex().IsMatch(username);
    }

    public static bool IsEmailValid(string email)
    {
        return EmailRegex().IsMatch(email);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9._]{2,19}$")]
    private static partial Regex UsernameRegex();

    [GeneratedRegex(@"^[\w\.\-]+@([\w\-]+\.)+[\w]{2,}$")]
    private static partial Regex EmailRegex();
}