namespace lumires.Api.Domain.Entities;

public class Movie
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public int Year { get; set; }
}