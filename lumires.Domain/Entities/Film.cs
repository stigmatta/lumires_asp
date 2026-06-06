using lumires.Domain.Base;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class Film : LikeableEntity<FilmLike>
{
    private readonly List<FilmCast> _cast = [];
    private readonly List<FilmDirector> _directors = [];
    private readonly List<Genre> _genres = [];
    private readonly List<FilmLocalization> _localizations = [];
    private readonly List<Review> _reviews = [];
    private readonly List<FilmTag> _tags = [];
    private readonly List<UserFilmRating> _userRatings = [];

    private Film()
    {
    }

    public Film(int externalId, DateOnly? releaseDate, string? posterPath, float voteAverage,
        int voteCount, float popularity, int runtime, string productionCompany, string? backdropPath = null,
        string? trailerUrl = null)
        : this(Guid.CreateVersion7(), externalId, releaseDate, posterPath, voteAverage, voteCount, popularity,
            runtime, productionCompany, backdropPath, trailerUrl)
    {
    }

    public Film(Guid id, int externalId, DateOnly? releaseDate, string? posterPath, float voteAverage,
        int voteCount, float popularity, int runtime, string productionCompany, string? backdropPath = null,
        string? trailerUrl = null)
    {
        if (externalId <= 0)
            throw new DomainException("ExternalId must be positive", nameof(externalId));

        if (releaseDate.HasValue &&
            (releaseDate < new DateOnly(1888, 1, 1) || releaseDate > new DateOnly(2126, 12, 31)))
            throw new DomainException("Invalid movie release date", nameof(releaseDate));

        if (voteAverage is < 0 or > 5)
            throw new DomainException("Invalid average vote rating", nameof(voteAverage));

        if (voteCount < 0)
            throw new DomainException("Invalid vote count", nameof(voteCount));

        if (popularity < 0)
            throw new DomainException("Invalid popularity", nameof(popularity));

        Id = id;
        ExternalId = externalId;
        ReleaseDate = releaseDate;
        PosterPath = posterPath;
        VoteAverage = voteAverage;
        VoteCount = voteCount;
        Popularity = popularity;
        BackdropPath = backdropPath;
        TrailerUrl = trailerUrl;
        Runtime = runtime;
        ProductionCompany = productionCompany;
    }

    public Guid Id { get; }
    public int ExternalId { get; }
    public string Slug { get; private set; } = null!;
    public DateOnly? ReleaseDate { get; private set; }
    public string? PosterPath { get; private set; }
    public string? BackdropPath { get; private set; }
    public string? TrailerUrl { get; private set; }
    public float VoteAverage { get; private set; }
    public int VoteCount { get; private set; }
    public float Popularity { get; private set; }
    public int Runtime { get; private set; }
    public string ProductionCompany { get; private set; } = null!;
    public bool IsEditorPick { get; private set; }


    public IReadOnlyCollection<Genre> Genres => _genres.AsReadOnly();
    public IReadOnlyCollection<FilmLocalization> Localizations => _localizations.AsReadOnly();
    public IReadOnlyCollection<FilmCast> Cast => _cast.AsReadOnly();
    public IReadOnlyCollection<FilmDirector> Directors => _directors.AsReadOnly();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    public IReadOnlyCollection<UserFilmRating> UserRatings => _userRatings.AsReadOnly();
    public IReadOnlyCollection<FilmTag> Tags => _tags.AsReadOnly();


    protected override Guid GetUserId(FilmLike like)
    {
        return like.UserId;
    }

    protected override FilmLike CreateLike(Guid userId)
    {
        return new FilmLike { FilmId = Id, UserId = userId, LikedAt = DateTimeOffset.Now };
    }


    public void AddLocalization(FilmLocalization localization)
    {
        ArgumentNullException.ThrowIfNull(localization);

        if (_localizations.Any(l => l.LanguageCode == localization.LanguageCode))
            throw new DomainException("Localization already exists for this language");

        localization.SetFilm(this);
        _localizations.Add(localization);
    }

    public void AddSlug(string slug)
    {
        ArgumentNullException.ThrowIfNull(slug);
        Slug = slug;
    }

    public void AddGenres(IEnumerable<Genre> genres)
    {
        ArgumentNullException.ThrowIfNull(genres);

        foreach (var genre in genres)
        {
            if (_genres.Any(g => g.Id == genre.Id))
                throw new DomainException($"Genre '{genre.Id}' already added to this movie");

            _genres.Add(genre);
        }
    }

    public void AddReview(Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        _reviews.Add(review);
    }

    public void SyncGenres(IEnumerable<Genre> genres)
    {
        _genres.Clear();
        foreach (var genre in genres) _genres.Add(genre);
    }

    public void AddCast(FilmCast cast)
    {
        ArgumentNullException.ThrowIfNull(cast);

        if (_cast.Any(c => c.PersonId == cast.Id))
            throw new DomainException($"Actor '{cast.Id}' already added to this movie");

        _cast.Add(cast);
    }

    public void AddDirector(FilmDirector director)
    {
        ArgumentNullException.ThrowIfNull(director);

        if (_directors.Any(c => c.Id == director.Id))
            throw new DomainException($"Director '{director.Id}' already added to this movie");

        _directors.Add(director);
    }

    public void SetEditorPick(bool editorPick)
    {
        IsEditorPick = editorPick;
    }

    public void AddTag(Tag tag)
    {
        if (_tags.Any(t => t.TagId == tag.Id)) return;
        _tags.Add(new FilmTag { FilmId = Id, TagId = tag.Id });
    }
}