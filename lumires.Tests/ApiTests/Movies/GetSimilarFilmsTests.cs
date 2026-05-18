using Ardalis.Result;
using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.GetSimilarFilms;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Movies;

internal sealed class GetSimilarFilmsTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<IExternalFilmService> _externalFilmServiceMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.LangCulture).Returns("en-US");

        _externalFilmServiceMock = new Mock<IExternalFilmService>();

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.Films)
            .Returns(new List<Film>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _externalFilmServiceMock.Object,
            dataAccess ?? _dataAccess);
    }

    // -------------------------------------------------------------------------
    // Error responses
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetSimilarFilms_Should_Be_401_WhenServiceReturnsUnauthorized()
    {
        // Arrange
        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ExternalFilmShort>>.Unauthorized());

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(42), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(401);
    }

    [Test]
    public async Task GetSimilarFilms_Should_Be_404_WhenServiceReturnsNotFound()
    {
        // Arrange
        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ExternalFilmShort>>.NotFound());

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(42), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task GetSimilarFilms_Should_Not_QueryDb_WhenServiceFails()
    {
        // Arrange
        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ExternalFilmShort>>.NotFound());

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(42), CancellationToken.None);

        // Assert
        _dbContextMock.Verify(x => x.Films, Times.Never);
    }


    [Test]
    [Arguments(42, 1, "/back1.jpg", "/poster1.jpg", 2020, 7.5f, 351, 25f)]
    [Arguments(550, 2, "/back2.jpg", "/poster2.jpg", 2018, 8.1f, 228, 21f)]
    public async Task GetSimilarFilms_Should_Be_200_WhenServiceSucceeds(
        int queryId, int externalId, string backdropPath, string posterPath,
        int releaseYear, float voteAverage, int voteCount, float popularity)
    {
        // Arrange
        var films = new List<ExternalFilmShort>
        {
            new(externalId, $"Film {externalId}", posterPath, releaseYear, voteAverage, voteCount, popularity)
        };

        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(queryId, "en-US", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ExternalFilmShort>>(films));

        _dbContextMock
            .Setup(x => x.Films)
            .Returns(new List<Film>().BuildMockDbSet().Object);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(queryId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Films.Should().ContainSingle()
            .Which.ExternalId.Should().Be(externalId);
    }

    [Test]
    [Arguments(42, 3)]
    [Arguments(550, 5)]
    public async Task GetSimilarFilms_Should_ReturnAllFilmsFromService(int queryId, int filmCount)
    {
        // Arrange
        var films = Enumerable.Range(1, filmCount)
            .Select(i => new ExternalFilmShort(i, $"Film {i}", null, 1995, 7f, 23, 25))
            .ToList();

        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ExternalFilmShort>>(films));

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(queryId), CancellationToken.None);

        // Assert
        ep.Response.Films.Should().HaveCount(filmCount);
    }


    [Test]
    [Arguments(42, "en-US")]
    [Arguments(550, "uk-UA")]
    public async Task GetSimilarFilms_Should_PassLangCulture_ToService(int queryId, string lang)
    {
        // Arrange
        _currentUserMock.Setup(x => x.LangCulture).Returns(lang);
        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ExternalFilmShort>>.NotFound());

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(queryId), CancellationToken.None);

        // Assert
        _externalFilmServiceMock.Verify(
            x => x.GetSimilarFilmsAsync(queryId, lang, It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    public async Task GetSimilarFilms_Should_QueryDb_WithAllReturnedFilmIds()
    {
        // Arrange
        var films = new List<ExternalFilmShort>
        {
            new(10, "Film A", null, 2005, 8, 27, 20),
            new(20, "Film B", null, 1999, 8, 27, 20),
            new(30, "Film C", null, 2003, 8, 27, 20)
        };

        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ExternalFilmShort>>(films));

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(99), CancellationToken.None);

        // Assert
        _dbContextMock.Verify(x => x.Films, Times.Once);
    }

    [Test]
    public async Task GetSimilarFilms_Should_NotQueryDb_WhenServiceReturnsEmptyList()
    {
        // Arrange
        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ExternalFilmShort>>([]));

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(42), CancellationToken.None);

        // Assert
        _dbContextMock.Verify(x => x.Films, Times.Never);
    }

    [Test]
    public async Task GetSimilarFilms_Should_OnlyEnrich_FilmsNotAlreadyInDb()
    {
        // Arrange 
        var films = new List<ExternalFilmShort>
        {
            new(10, "Existing A", null, 1952, 8, 25, 20),
            new(20, "Existing B", null, 1932, 8, 25, 63),
            new(30, "New Film", null, 2004, 8, 25, 12)
        };

        _externalFilmServiceMock
            .Setup(x => x.GetSimilarFilmsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ExternalFilmShort>>(films));

        var existingFilms = new List<Film>
        {
            CreateFilm(10),
            CreateFilm(20)
        };

        _dbContextMock
            .Setup(x => x.Films)
            .Returns(existingFilms.BuildMockDbSet().Object);

        var dataAccess = new DataAccess(_dbContextMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act 
        var act = async () => await ep.HandleAsync(new Query(99), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        ep.HttpContext.Response.StatusCode.Should().Be(200);
    }

    private static Film CreateFilm(int externalId)
    {
        return new Film(
            externalId,
            new DateOnly(2000, 1, 1),
            null,
            0f,
            0,
            0f,
            90,
            "Test"
        );
    }
}