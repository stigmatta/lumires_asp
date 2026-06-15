using System.Text.RegularExpressions;
using lumires.Domain.Enums;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed partial class User
{
    private readonly List<UserFilmRating> _filmRatings = [];
    private readonly List<FilmsList> _filmsLists = [];
    private readonly List<UsersRelationship> _incomingRelationships = [];
    private readonly List<UsersRelationship> _outgoingRelationships = [];
    private readonly List<ReviewComment> _reviewComments = [];
    private readonly List<Review> _reviews = [];
    private readonly List<SavedFilm> _savedFilms = [];
    private readonly List<SavedList> _savedLists = [];
    private readonly List<UserThreadComment> _userThreadComments = [];
    private readonly List<UserThread> _userThreads = [];
    private readonly List<WatchedFilm> _watchedFilms = [];
    private readonly List<WatchlistFilm> _watchlistFilms = [];

    private User()
    {
    }

    public User(Guid id, string? username, string email)
    {
        if (id == Guid.Empty) throw new DomainException("UserId is invalid", nameof(id));

        if (!IsEmailValid(email))
            throw new DomainException("Email is not valid", nameof(email));

        if (string.IsNullOrWhiteSpace(username)) username = email.Split('@')[0];

        if (!IsUsernameValid(username))
            throw new DomainException("Username contains invalid characters or starts incorrectly.", nameof(username));

        Id = id;
        Username = username;
        Email = email;
        CreatedAt = DateTimeOffset.UtcNow;
        UserSettings = new UserSettings(id);
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string? DisplayName { get; private set; }
    public string Email { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? LastActiveAt { get; private set; }
    public UserPronouns Pronouns { get; private set; }
    public string? Location { get; private set; }
    public string? Tagline { get; private set; }
    public string? Biography { get; private set; }
    public UserSettings UserSettings { get; private set; } = null!;

    public IReadOnlyCollection<FilmsList> FilmsLists => _filmsLists.AsReadOnly();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    public IReadOnlyCollection<ReviewComment> ReviewComments => _reviewComments.AsReadOnly();
    public IReadOnlyCollection<UserFilmRating> FilmRatings => _filmRatings.AsReadOnly();
    public IReadOnlyCollection<UserThread> UserThreads => _userThreads.AsReadOnly();
    public IReadOnlyCollection<UserThreadComment> UserThreadsComments => _userThreadComments.AsReadOnly();

    public IReadOnlyCollection<UsersRelationship> OutgoingRelationships =>
        _outgoingRelationships.AsReadOnly();

    public IReadOnlyCollection<UsersRelationship> IncomingRelationships =>
        _incomingRelationships.AsReadOnly();

    public IReadOnlyCollection<WatchedFilm> WatchedFilms =>
        _watchedFilms.AsReadOnly();

    public IReadOnlyCollection<WatchlistFilm> WatchlistFilms =>
        _watchlistFilms.AsReadOnly();

    public IReadOnlyCollection<SavedFilm> SavedFilms =>
        _savedFilms.AsReadOnly();

    public IReadOnlyCollection<SavedList> SavedLists =>
        _savedLists.AsReadOnly();

    public void UpdateProfileSettings(string? avatarUrl, string? displayName, string? username, UserPronouns pronouns,
        string? location, string? tagline, string? biography)
    {
        AvatarUrl = avatarUrl;
        DisplayName = displayName;
        Pronouns = pronouns;
        Location = location;
        Tagline = tagline;
        Biography = biography;
        
        if (username is not null)
        {
            Username = username;
        }
    }
    

    public void SetAvatarUrl(string avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarUrl);
        AvatarUrl = avatarUrl;
    }

    public void SetEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        AvatarUrl = email;
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