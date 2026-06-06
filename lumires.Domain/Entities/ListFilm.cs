using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class ListFilm
{
    private ListFilm()
    {
    }

    public ListFilm(Guid filmsListId, Guid filmId, int order)
    {
        if (filmsListId == Guid.Empty) throw new DomainException("Collection ID is invalid", nameof(filmsListId));

        if (filmId == Guid.Empty) throw new DomainException("Movie ID is invalid", nameof(filmId));

        if (order < 0) throw new DomainException("Order cannot be negative", nameof(order));

        FilmsListId = filmsListId;
        FilmId = filmId;
        Order = order;
        AddedAt = DateTimeOffset.UtcNow;
    }

    public ListFilm(Guid filmsListId, Film film, int order)
    {
        if (filmsListId == Guid.Empty) throw new DomainException("Collection ID is invalid", nameof(filmsListId));

        if (film is null) throw new DomainException("Movie cannot be null", nameof(film));

        if (order < 0) throw new DomainException("Order cannot be negative", nameof(order));

        FilmsListId = filmsListId;
        FilmId = film.Id;
        Film = film;
        Order = order;
        AddedAt = DateTimeOffset.UtcNow;
    }

    public Guid FilmsListId { get; private set; }
    public Guid FilmId { get; private set; }
    public int Order { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    public FilmsList FilmsList { get; private set; } = null!;
    public Film Film { get; private set; } = null!;

    public void SetFilm(Film film)
    {
        ArgumentNullException.ThrowIfNull(film);

        Film = film;
    }
}