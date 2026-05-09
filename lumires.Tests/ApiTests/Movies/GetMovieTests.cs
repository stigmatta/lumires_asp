using Ardalis.Result;
using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Movies.GetMovie;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Movies;
using lumires.Core.Models;
using lumires.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace Tests.ApiTests.Movies;

internal sealed class GetMovieTests
{
    private FusionCache _cache = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IExternalMovieService> _externalMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _externalMock = new Mock<IExternalMovieService>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.LangCulture)
            .Returns("en");

        _cache = new FusionCache(new FusionCacheOptions());

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(new List<Movie>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(dbContextMock.Object);
    }

    [After(Test)]
    public void TearDown()
    {
        _cache.Dispose();
    }

    [Test]
    [Arguments(1, "en")]
    [Arguments(0, "en")]
    public async Task GetMovie_Should_Be_404_When_NotFound(int tmdbId, string region)
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(
                tmdbId,
                region,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.NotFound());

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dataAccess);

        // Act
        await ep.HandleAsync(new Query(tmdbId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(tmdbId, region, It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20)]
    public async Task GetMovie_Should_Be_200_And_MapDataCorrectly(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var releaseDate = DateOnly.Parse(dateStr);
        var genres = Helpers.CreateExternalGenres();
        var externalMovie =
            Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity, releaseDate, genres);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, _dataAccess);

        await ep.HandleAsync(new Query(id), CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.ReleaseDate.Should().Be(releaseDate);
        ep.Response.PosterPath.Should().Be(poster);
        ep.Response.Localization!.Title.Should().Be(title);
        ep.Response.Genres.Items.Should().HaveCount(2);
        ep.Response.Genres.Items.Select(g => g.Name).Should().BeEquivalentTo("Action", "Drama");

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovie_Should_Be_401_When_Unauthorized()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Unauthorized());

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dataAccess);

        // Act
        await ep.HandleAsync(new Query(It.IsAny<int>()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(401);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovie_Should_Be_500_When_Service_Error()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Error());

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dataAccess);

        // Act
        await ep.HandleAsync(new Query(It.IsAny<int>()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(500);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20)]
    public async Task GetMovie_Should_CallExternalService_OnlyOnce_When_CalledTwice(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var releaseDate = DateOnly.Parse(dateStr);
        var externalMovie =
            Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity, releaseDate);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, _dataAccess);

        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovie_Should_NotCache_When_NotFound()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.NotFound());

        const int id = 1;

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dataAccess);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }


    [Test]
    public async Task GetMovie_Should_NotCache_When_Unauthorized()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Unauthorized());

        const int id = 1;


        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dataAccess);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task GetMovie_Should_NotCache_When_Service_Error()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Error());

        const int id = 1;


        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dataAccess);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20)]
    public async Task GetMovie_Should_ReturnCachedResponse_On_SecondCall(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var releaseDate = DateOnly.Parse(dateStr);
        var externalMovie =
            Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity, releaseDate);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, _dataAccess);

        await ep.HandleAsync(new Query(id), CancellationToken.None);
        var firstResponse = ep.Response;

        await ep.HandleAsync(new Query(id), CancellationToken.None);
        var secondResponse = ep.Response;

        secondResponse.Should().BeEquivalentTo(firstResponse);
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20)]
    public async Task GetMovie_Should_CacheSeparately_Per_Language(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var releaseDate = DateOnly.Parse(dateStr);
        var externalMovie =
            Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity, releaseDate);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var enUserMock = new Mock<ICurrentUserService>();
        enUserMock.Setup(x => x.LangCulture).Returns("en");

        var uaUserMock = new Mock<ICurrentUserService>();
        uaUserMock.Setup(x => x.LangCulture).Returns("uk-UA");

        var ep1 = Factory.Create<Endpoint>(_externalMock.Object, enUserMock.Object, _cache, _dataAccess);
        var ep2 = Factory.Create<Endpoint>(_externalMock.Object, uaUserMock.Object, _cache, _dataAccess);

        await ep1.HandleAsync(new Query(id), CancellationToken.None);
        await ep1.HandleAsync(new Query(id), CancellationToken.None);
        await ep2.HandleAsync(new Query(id), CancellationToken.None);
        await ep2.HandleAsync(new Query(id), CancellationToken.None);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }


    [Test]
    [Arguments(1, "2010-07-16", "/poster1.jpg", 4.5, 200, 20)]
    [Arguments(42, "2014-11-07", "/poster2.jpg", 3.8, 350, 20)]
    public async Task GetMovie_Should_NotCallExternalService_When_FoundInDb(
        int externalId,
        string dateStr,
        string posterPath,
        float voteAverage,
        int voteCount,
        float popularity)
    {
        // Arrange 
        var releaseDate = DateOnly.Parse(dateStr);

        var movies = new List<Movie>
        {
            new(externalId, releaseDate, posterPath, voteAverage, voteCount, popularity)
        }.BuildMockDbSet();

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.Object);

        var dbQueries = new DataAccess(dbContextMock.Object);
        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, dbQueries);

        // Act
        await ep.HandleAsync(new Query(externalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.ReleaseDate.Should().Be(releaseDate);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20)]
    public async Task GetMovie_Should_CallExternalService_When_NotFoundInDb(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var releaseDate = DateOnly.Parse(dateStr);
        var movies = new List<Movie>().BuildMockDbSet();

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock.Setup(x => x.Movies).Returns(movies.Object);

        var dbQueries = new DataAccess(dbContextMock.Object);
        var externalMovie =
            Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity, releaseDate);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, dbQueries);

        await ep.HandleAsync(new Query(id), CancellationToken.None);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg", 3.8, 350, 20)]
    public async Task GetMovie_Should_Publish_MovieReferencedEvent(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var fakeHandler = new FakeMovieReferencedEventHandler();
        var releaseDate = DateOnly.Parse(dateStr);
        var externalMovie =
            Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity, releaseDate);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                s.AddSingleton(_externalMock.Object);
                s.AddSingleton(_currentUserMock.Object);
                s.AddSingleton<IFusionCache>(_cache);
                s.AddSingleton(_dataAccess);
                s.AddSingleton<IEventHandler<MovieReferencedEvent>>(fakeHandler);
            });
        });

        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await fakeHandler.Completed.WaitAsync(TimeSpan.FromSeconds(2));

        fakeHandler.WasHandled.Should().BeTrue();
        fakeHandler.MovieId.Should().Be(id);
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    public async Task GetMovie_Should_MapGenres_Correctly(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var releaseDate = DateOnly.Parse(dateStr);
        var genres = new ExternalGenres([
            new ExternalGenreItem(28, "Action"),
            new ExternalGenreItem(18, "Drama"),
            new ExternalGenreItem(53, "Thriller")
        ]);
        var externalMovie =
            Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity, releaseDate, genres);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, _dataAccess);

        await ep.HandleAsync(new Query(id), CancellationToken.None);

        ep.Response.Genres.Items.Should().HaveCount(3);
        ep.Response.Genres.Items.Select(g => g.Name)
            .Should().BeEquivalentTo("Action", "Drama", "Thriller");
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg", 4.5, 200, 20)]
    public async Task GetMovie_Should_Return_EmptyGenres_When_NoGenres(
        int id, string title, string dateStr, string poster,
        float voteAverage, int voteCount, float popularity)
    {
        var releaseDate = DateOnly.Parse(dateStr);
        var externalMovie = Helpers.CreateExternalMovie(id, title, poster, voteAverage, voteCount, popularity,
            releaseDate,
            new ExternalGenres([]));

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, _dataAccess);

        await ep.HandleAsync(new Query(id), CancellationToken.None);

        ep.Response.Genres.Items.Should().BeEmpty();
    }

    private sealed class FakeMovieReferencedEventHandler : IEventHandler<MovieReferencedEvent>
    {
        private readonly TaskCompletionSource _tcs = new();

        public Task Completed => _tcs.Task;
        public bool WasHandled { get; private set; }
        public int MovieId { get; private set; }

        public Task HandleAsync(MovieReferencedEvent e, CancellationToken c)
        {
            WasHandled = true;
            MovieId = e.ExternalId;
            _tcs.SetResult();

            return Task.CompletedTask;
        }
    }
}