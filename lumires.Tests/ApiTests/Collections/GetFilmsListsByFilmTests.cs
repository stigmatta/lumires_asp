using System.Reflection;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.GetFilmsListsByFilmPreview;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Base;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class GetFilmsListsByFilmTests
{
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

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
            BuildFilmsList("High", 100, [(43, "/h.jpg")]),
            BuildFilmsList("Medium", 50, [(34, "/m.jpg")]),
            BuildFilmsList("Low", 1, [(45, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(42, CancellationToken.None);

        // Assert
        result.FilmsLists.Should().BeEmpty();
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_WhenListsContainFilm_ReturnsMatchingLists()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High", 100, [(43, "/h.jpg")]),
            BuildFilmsList("Medium", 50, [(34, "/m.jpg")]),
            BuildFilmsList("Low", 1, [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(42, CancellationToken.None);

        // Assert
        result.FilmsLists.Should().ContainSingle()
            .Which.Name.Should().Be("Low");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_ReturnsCorrectBackdropPath()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High", 100, [(43, "/h.jpg")]),
            BuildFilmsList("Medium", 50, [(34, "/m.jpg")]),
            BuildFilmsList("Low", 1, [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(42, CancellationToken.None);

        // Assert
        result.FilmsLists.Should().ContainSingle()
            .Which.Films.Should().ContainSingle()
            .Which.BackdropPath.Should().Be("/l.jpg");
    }


    [Test]
    public async Task GetFilmListsByFilmIdAsync_ExcludesFilmsFromOtherListsThatDontContainTargetFilm()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High", 100, [(43, "/h.jpg")]),
            BuildFilmsList("Medium", 50, [(34, "/m.jpg")]),
            BuildFilmsList("Low", 1, [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(42, CancellationToken.None);

        // Assert
        result.FilmsLists.Should().ContainSingle()
            .Which.Name.Should().Be("Low");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_OrdersByLikesCountDescending()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High", 100, [(42, "/h.jpg")]),
            BuildFilmsList("Medium", 50, [(42, "/m.jpg")]),
            BuildFilmsList("Low", 1, [(42, "/l.jpg")])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(42, CancellationToken.None);

        // Assert
        result.FilmsLists.Select(x => x.Name)
            .Should().ContainInOrder("High", "Medium", "Low");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_ReturnsAtMostFourLists()
    {
        // Arrange — 6 lists all containing the target film
        var lists = Enumerable.Range(1, 6)
            .Select(i => BuildFilmsList(
                $"List {i}",
                i,
                [(42, $"/backdrop{i}.jpg")]
            ))
            .ToList();

        SetupFilmsLists(lists);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(42, CancellationToken.None);

        // Assert
        result.FilmsLists.Should().HaveCount(4,
            "the query applies Take(4)");
    }

    [Test]
    public async Task GetFilmListsByFilmIdAsync_WhenBackdropPathIsNull_ReturnsNullBackdropPath()
    {
        // Arrange
        SetupFilmsLists([
            BuildFilmsList("High", 100, [(42, null)])
        ]);

        // Act
        var result = await _dataAccess.GetFilmListsByFilmIdAsync(42, CancellationToken.None);

        // Assert
        result.FilmsLists.Should().ContainSingle()
            .Which.Films.Should().ContainSingle()
            .Which.BackdropPath.Should().BeNull();
    }


    private static Film CreateFilm(int externalId, string? backdropPath)
    {
        return new Film(
            externalId,
            new DateOnly(2000, 1, 1),
            null,
            0f,
            0,
            0f,
            90,
            "Test",
            backdropPath
        );
    }

    private static ListFilm CreateListFilm(Guid filmsListId, Film film)
    {
        var listFilm = new ListFilm(filmsListId, film.Id, 1);

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
        var list = new FilmsList(title, Guid.NewGuid());

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