using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.GetFilm;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace Tests.ApiTests.Movies;

internal sealed class GetMovieTests
{
    private FusionCache _cache = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IFilmResolver> _resolverMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.LangCulture).Returns("en");

        _resolverMock = new Mock<IFilmResolver>();
        _resolverMock
            .Setup(x => x.EnsureFilmExistsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cache = new FusionCache(new FusionCacheOptions());

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.Films)
            .Returns(new List<Film>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [After(Test)]
    public void TearDown()
    {
        _cache.Dispose();
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null, IFilmResolver? resolver = null)
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            resolver ?? _resolverMock.Object,
            _cache,
            dataAccess ?? _dataAccess);
    }

    [Test]
    [Arguments(1)]
    [Arguments(0)]
    public async Task GetMovie_Should_Be_404_When_NotFound(int tmdbId)
    {
        // Arrange — resolver вернул false, фильм не скачался
        _resolverMock
            .Setup(x => x.EnsureFilmExistsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(tmdbId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    [Arguments(1, "2010-07-16", "/poster1.jpg", 4.5, 200, 20, 105, "HBO")]
    [Arguments(42, "2014-11-07", "/poster2.jpg", 3.8, 350, 20, 168, "HBO")]
    public async Task GetMovie_Should_Be_200_When_FoundInDb(
        int externalId, string dateStr, string posterPath,
        float voteAverage, int voteCount, float popularity, int runtime, string productionCompany)
    {
        // Arrange
        var releaseDate = DateOnly.Parse(dateStr);
        var movie = new Film(externalId, releaseDate, posterPath, voteAverage, voteCount, popularity, runtime, productionCompany);
        var movies = new List<Film> { movie }.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Films).Returns(movies.Object);
        var dataAccess = new DataAccess(_dbContextMock.Object);

        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(new Query(externalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.ReleaseDate.Should().Be(releaseDate);
        ep.Response.PosterPath.Should().Be(posterPath);
    }

    [Test]
    [Arguments(2)]
    [Arguments(500)]
    public async Task GetMovie_Should_CallResolver_When_NotFoundInDb(int id)
    {
        // Arrange — пустая БД
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _resolverMock.Verify(
            x => x.EnsureFilmExistsAsync(id, It.IsAny<string>(),It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20, 105, "HBO")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20, 107, "HBO")]
    public async Task GetMovie_Should_ReturnCachedResponse_On_SecondCall(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity, int runtime, string productionCompany)
    {
        // Arrange
        var releaseDate = DateOnly.Parse(dateStr);
        var movie = new Film(id, releaseDate, poster, voteAverage, voteCount, popularity, runtime, productionCompany);
        var movies = new List<Film> { movie }.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Films).Returns(movies.Object);
        var dataAccess = new DataAccess(_dbContextMock.Object);

        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        var firstResponse = ep.Response;

        await ep.HandleAsync(new Query(id), CancellationToken.None);
        var secondResponse = ep.Response;

        // Assert
        secondResponse.Should().BeEquivalentTo(firstResponse);
        _resolverMock.Verify(
            x => x.EnsureFilmExistsAsync(id, It.IsAny<string>(),It.IsAny<CancellationToken>()),
            Times.Once); // второй раз из кэша
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20, 105, "HBO")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20, 107, "HBO")]
    public async Task GetMovie_Should_CacheSeparately_Per_Language(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity, int runtime, string productionCompany)
    {
        // Arrange
        var releaseDate = DateOnly.Parse(dateStr);
        var movie = new Film(id, releaseDate, poster, voteAverage, voteCount, popularity, runtime, productionCompany);
        var movies = new List<Film> { movie }.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Films).Returns(movies.Object);
        var dataAccess = new DataAccess(_dbContextMock.Object);

        var enUserMock = new Mock<ICurrentUserService>();
        enUserMock.Setup(x => x.LangCulture).Returns("en");

        var uaUserMock = new Mock<ICurrentUserService>();
        uaUserMock.Setup(x => x.LangCulture).Returns("uk-UA");

        var ep1 = Factory.Create<Endpoint>(enUserMock.Object, _resolverMock.Object, _cache, dataAccess);
        var ep2 = Factory.Create<Endpoint>(uaUserMock.Object, _resolverMock.Object, _cache, dataAccess);

        // Act
        await ep1.HandleAsync(new Query(id), CancellationToken.None);
        await ep1.HandleAsync(new Query(id), CancellationToken.None);
        await ep2.HandleAsync(new Query(id), CancellationToken.None);
        await ep2.HandleAsync(new Query(id), CancellationToken.None);

        // Assert — resolver вызван дважды (по одному на язык)
        _resolverMock.Verify(
            x => x.EnsureFilmExistsAsync(id, It.IsAny<string>(),It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

}

