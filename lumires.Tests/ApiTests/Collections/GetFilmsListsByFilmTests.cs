using System.Reflection;
using lumires.Api.Features.FilmsLists.GetFilmsListsByFilm;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using FluentAssertions;
using lumires.Domain.Base;

namespace Tests.ApiTests.Collections;

internal sealed class GetFilmsListsByFilmTests
{
    private Mock<IAppDbContext> _dbContextMock = null!;
    private DataAccess _dataAccess = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private void SetupFilmsLists(List<FilmsList> data)
    {
        _dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(data.BuildMockDbSet().Object);
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_WhenNoListsContainFilm_ReturnsEmptyCollection()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High",   likesCount: 100, films: [(43, "/h.jpg")]),
            BuildFilmsList("Medium", likesCount: 50,  films: [(34, "/m.jpg")]),
            BuildFilmsList("Low",    likesCount: 1,   films: [(45, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(filmId: 42, ct: CancellationToken.None);

        // Assert
        result.FilmLists.Should().BeEmpty();
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_WhenListsContainFilm_ReturnsMatchingLists()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High",   likesCount: 100, films: [(43, "/h.jpg")]),
            BuildFilmsList("Medium", likesCount: 50,  films: [(34, "/m.jpg")]),
            BuildFilmsList("Low",    likesCount: 1,   films: [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(filmId: 42, ct: CancellationToken.None);

        // Assert
        result.FilmLists.Should().ContainSingle()
            .Which.Name.Should().Be("Low");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_ReturnsCorrectBackdropPath()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High",   likesCount: 100, films: [(43, "/h.jpg")]),
            BuildFilmsList("Medium", likesCount: 50,  films: [(34, "/m.jpg")]),
            BuildFilmsList("Low",    likesCount: 1,   films: [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(filmId: 42, ct: CancellationToken.None);

        // Assert
        result.FilmLists.Should().ContainSingle()
            .Which.Films.Should().ContainSingle()
            .Which.BackdropPath.Should().Be("/l.jpg");
    }


    [Test]
    public async Task GetFilmListsByFilmIdAsync_ExcludesFilmsFromOtherListsThatDontContainTargetFilm()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High",   likesCount: 100, films: [(43, "/h.jpg")]),
            BuildFilmsList("Medium", likesCount: 50,  films: [(34, "/m.jpg")]),
            BuildFilmsList("Low",    likesCount: 1,   films: [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(filmId: 42, ct: CancellationToken.None);

        // Assert
        result.FilmLists.Should().ContainSingle()
            .Which.Name.Should().Be("Low");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_OrdersByLikesCountDescending()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High",   likesCount: 100, films: [(42, "/h.jpg")]),
            BuildFilmsList("Medium", likesCount: 50,  films: [(42, "/m.jpg")]),
            BuildFilmsList("Low",    likesCount: 1,   films: [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(filmId: 42, ct: CancellationToken.None);

        // Assert
        result.FilmLists.Select(x => x.Name)
            .Should().ContainInOrder("High", "Medium", "Low");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_ReturnsAtMostFourLists()
    {
        // Arrange — 6 lists all containing the target film
        var lists = Enumerable.Range(1, 6)
            .Select(i => BuildFilmsList(
                title: $"List {i}",
                likesCount: i,
                films: [(42, $"/backdrop{i}.jpg")]
            ))
            .ToList();

        SetupFilmsLists(lists);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(filmId: 42, ct: CancellationToken.None);

        // Assert
        result.FilmLists.Should().HaveCount(4,
            because: "the query applies Take(4)");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_WhenBackdropPathIsNull_ReturnsNullBackdropPath()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High",   likesCount: 100, films: [(42, null)]),
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(filmId: 42, ct: CancellationToken.None);

        // Assert
        result.FilmLists.Should().ContainSingle()
            .Which.Films.Should().ContainSingle()
            .Which.BackdropPath.Should().BeNull();
    }


    private static Film CreateFilm(int externalId, string? backdropPath) =>
        new(
            externalId:        externalId,
            releaseDate:       new DateOnly(2000, 1, 1),
            posterPath:        null,
            voteAverage:       0f,
            voteCount:         0,
            popularity:        0f,
            runtime:           90,
            productionCompany: "Test",
            backdropPath:      backdropPath
        );

    private static ListFilm CreateListFilm(Guid filmsListId, Film film)
    {
        var listFilm = new ListFilm(filmsListId, film.Id, order: 1);

        typeof(ListFilm)
            .GetProperty(nameof(ListFilm.Film))!
            .SetValue(listFilm, film);

        return listFilm;
    }

    private static FilmsList BuildFilmsList(
        string title,
        int likesCount,
        IEnumerable<(int ExternalId, string? BackdropPath)> films)
    {
        var list = new FilmsList(title, userId: Guid.NewGuid());

        var filmsField = typeof(FilmsList)
            .GetField("_films", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var backingList = (List<ListFilm>)filmsField.GetValue(list)!;

        foreach (var (externalId, backdropPath) in films)
            backingList.Add(CreateListFilm(list.Id, CreateFilm(externalId, backdropPath)));

        SetLikesCount(list, likesCount);

        return list;
    }

    private static void SetLikesCount(FilmsList list, int likesCount)
    {
        var field = typeof(LikeableEntity<FilmsListLike>)
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .FirstOrDefault(f => f.FieldType == typeof(List<FilmsListLike>))
                    ?? throw new InvalidOperationException(
                        "No List<FilmsListLike> backing field found on LikeableEntity<FilmsListLike>.");

        var likes = (List<FilmsListLike>)field.GetValue(list)!;

        for (var i = 0; i < likesCount; i++)
            likes.Add(new FilmsListLike { FilmsListId = list.Id, UserId = Guid.NewGuid() });
    }
}