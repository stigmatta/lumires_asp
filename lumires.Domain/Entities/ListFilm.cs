using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class ListFilm
{
    private ListFilm()
    {
    }

    public ListFilm(Guid filmsListId, Guid filmId, int order)
    {
        if (filmsListId == Guid.Empty) throw new FilmsListValidationException("Collection ID is invalid");

        if (filmId == Guid.Empty) throw new FilmsListValidationException("Movie ID is invalid");

        if (order < 0) throw new FilmsListValidationException("Order cannot be negative");

        FilmsListId = filmsListId;
        FilmId = filmId;
        Order = order;
        AddedAt = DateTimeOffset.UtcNow;
    }

    public Guid FilmsListId { get; private set; }
    public Guid FilmId { get; private set; }
    public int Order { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    public FilmsList FilmsList { get; private set; } = null!;
    public Film Film { get; private set; } = null!;
}