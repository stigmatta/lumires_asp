namespace lumires.Domain.Entities;

public sealed class Tag
{
    private Tag()
    {
    }

    public Tag(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required", nameof(slug));

        Id = Guid.CreateVersion7();
        Name = name;
        Slug = slug;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; }
}