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
    private DbQueries _dbQueries = null!;
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

        _dbQueries = new DbQueries(dbContextMock.Object);
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
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(tmdbId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(tmdbId, region, It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg")]
    public async Task GetMovie_Should_Be_200_And_MapDataCorrectly(
        int id,
        string title,
        string dateStr,
        string poster)
    {
        // Arrange
        var releaseDate = DateTime.Parse(dateStr);
        var externalMovie = new ExternalMovie(
            id,
            title,
            null,
            poster,
            ReleaseDate: releaseDate,
            BackdropPath: null,
            TrailerUrl: null
        );

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);

        ep.Response.Id.Should().Be(id);
        ep.Response.Year.Should().Be(releaseDate.Year);
        ep.Response.PosterPath.Should().Be(poster);
        ep.Response.Localization!.Title.Should().Be(title);

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
            _dbQueries);

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
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(It.IsAny<int>()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(500);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg")]
    public async Task GetMovie_Should_CallExternalService_OnlyOnce_When_CalledTwice(
        int id,
        string title,
        string dateStr,
        string poster)
    {
        // Arrange
        var releaseDate = DateTime.Parse(dateStr);
        var externalMovie = new ExternalMovie(
            id,
            title,
            null,
            ReleaseDate: releaseDate,
            PosterPath: poster,
            BackdropPath: null,
            TrailerUrl: null
        );

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
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
            _dbQueries);

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
            _dbQueries);

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
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg")]
    public async Task GetMovie_Should_ReturnCachedResponse_On_SecondCall(
        int id,
        string title,
        string dateStr,
        string poster)
    {
        // Arrange
        var releaseDate = DateTime.Parse(dateStr);
        var externalMovie = new ExternalMovie(
            id,
            title,
            null,
            ReleaseDate: releaseDate,
            PosterPath: poster,
            BackdropPath: null,
            TrailerUrl: null
        );

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        var firstResponse = ep.Response;

        await ep.HandleAsync(new Query(id), CancellationToken.None);
        var secondResponse = ep.Response;

        // Assert
        secondResponse.Should().BeEquivalentTo(firstResponse);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg")]
    public async Task GetMovie_Should_CacheSeparately_Per_Language(
        int id,
        string title,
        string dateStr,
        string poster)
    {
        //Arrange
        var releaseDate = DateTime.Parse(dateStr);
        var externalMovie = new ExternalMovie(
            id,
            title,
            null,
            ReleaseDate: releaseDate,
            PosterPath: poster,
            BackdropPath: null,
            TrailerUrl: null
        );

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var enUserMock = new Mock<ICurrentUserService>();
        enUserMock.Setup(x => x.LangCulture).Returns("en");

        var uaUserMock = new Mock<ICurrentUserService>();
        uaUserMock.Setup(x => x.LangCulture).Returns("uk-UA");

        var ep1 = Factory.Create<Endpoint>(
            _externalMock.Object, enUserMock.Object, _cache, _dbQueries);

        var ep2 = Factory.Create<Endpoint>(
            _externalMock.Object, uaUserMock.Object, _cache, _dbQueries);

        // Act 
        await ep1.HandleAsync(new Query(id), CancellationToken.None);
        await ep1.HandleAsync(new Query(id), CancellationToken.None);

        await ep2.HandleAsync(new Query(id), CancellationToken.None);
        await ep2.HandleAsync(new Query(id), CancellationToken.None);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    [Arguments(1, 2020, "/poster1.jpg")]
    [Arguments(42, 1994, "/poster2.jpg")]
    public async Task GetMovie_Should_NotCallExternalService_When_FoundInDb(
        int externalId,
        int year,
        string posterPath)
    {
        // Arrange 
        var movies = new List<Movie>
        {
            new() { ExternalId = externalId, Year = year, PosterPath = posterPath }
        }.BuildMockDbSet();

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.Object);

        var dbQueries = new DbQueries(dbContextMock.Object);
        var ep = Factory.Create<Endpoint>(
            _externalMock.Object, _currentUserMock.Object, _cache, dbQueries);

        // Act
        await ep.HandleAsync(new Query(externalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Id.Should().Be(externalId);
        ep.Response.Year.Should().Be(year);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg")]
    public async Task GetMovie_Should_CallExternalService_When_NotFoundInDb(
        int id,
        string title,
        string dateStr,
        string poster)
    {
        // Arrange 
        var releaseDate = DateTime.Parse(dateStr);
        var movies = new List<Movie>().BuildMockDbSet();

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.Object);

        var dbQueries = new DbQueries(dbContextMock.Object);


        var externalMovie = new ExternalMovie(
            id,
            title,
            null,
            ReleaseDate: releaseDate,
            PosterPath: poster,
            BackdropPath: null,
            TrailerUrl: null
        );

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            dbQueries);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        ep.Response.Id.Should().Be(id);
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(id, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Arguments(2, "Inception", "2010-07-16", "/inc_poster.jpg")]
    [Arguments(500, "Interstellar", "2014-11-07", "/int_poster.jpg")]
    public async Task GetMovie_Should_Publish_MovieReferencedEvent(
        int id,
        string title,
        string dateStr,
        string poster)
    {
        // Arrange
        var fakeHandler = new FakeMovieReferencedEventHandler();
        var releaseDate = DateTime.Parse(dateStr);

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(
                new ExternalMovie(
                    id,
                    title,
                    null,
                    ReleaseDate: releaseDate,
                    PosterPath: poster,
                    BackdropPath: null,
                    TrailerUrl: null
                )
            ));

        var ep = Factory.Create<Endpoint>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                s.AddSingleton(_externalMock.Object);
                s.AddSingleton(_currentUserMock.Object);
                s.AddSingleton<IFusionCache>(_cache);
                s.AddSingleton(_dbQueries);
                s.AddSingleton<IEventHandler<MovieReferencedEvent>>(fakeHandler);
            });
        });

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await fakeHandler.Completed.WaitAsync(TimeSpan.FromSeconds(2));

        // Assert
        fakeHandler.WasHandled.Should().BeTrue();
        fakeHandler.MovieId.Should().Be(id);
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