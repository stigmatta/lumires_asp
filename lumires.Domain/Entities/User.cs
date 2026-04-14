using System.Text.RegularExpressions;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed partial class User
{
    private User()
    {
    }

    public User(Guid id, string username)
    {
        if (id == Guid.Empty) throw new UserValidationException("UserId is invalid");

        if (string.IsNullOrWhiteSpace(username))
            throw new UserValidationException("Username cannot be empty.");

        if (username.Length is < 3 or > 30)
            throw new UserValidationException("Username must be between 3 and 30 characters.");

        if (!UsernameRegex().IsMatch(username))
            throw new UserValidationException("Username contains invalid characters or starts incorrectly.");
        Id = id;
        Username = username;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public ICollection<Collection> Collections { get; private set; } = new List<Collection>();

    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9._]*$")]
    private static partial Regex UsernameRegex();
}