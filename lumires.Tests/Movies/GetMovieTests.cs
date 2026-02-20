using Api.Features.Movies.GetMovie;
using Ardalis.Result;
using Core.Abstractions.Data;
using Core.Abstractions.Services;
using Core.Events.Movies;
using Core.Models;
using Domain.Entities;
using FastEndpoints;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using ZiggyCreatures.Caching.Fusion;
using Endpoint = Api.Features.Movies.GetMovie.Endpoint;

namespace lumires.Tests.Movies;

public class GetMovieTests
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
    public async Task GetMovie_Should_Be_404_When_NotFound()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.NotFound());

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    public async Task GetMovie_Should_Be_200_When_Found()
    {
        // Arrange
        var externalMovie = new ExternalMovie
        (
            1,
            "Test Movie",
            "Test Overview",
            ReleaseDate: new DateTime(2020, 1, 1),
            PosterPath: "/poster.jpg",
            BackdropPath: "/backdrop.jpg",
            TrailerUrl: "https://youtube.com/test"
        );

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(externalMovie));

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Id.Should().Be(1);
        ep.Response.Year.Should().Be(2020);
        ep.Response.PosterPath.Should().Be("/poster.jpg");
        ep.Response.Localization!.Title.Should().Be("Test Movie");
        ep.Response.Localization.LanguageCode.Should().Be("en");

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
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
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(401);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
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
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(500);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }
    

    [Test]
    public async Task GetMovie_Should_CallExternalService_OnlyOnce_When_CalledTwice()
    {
        // Arrange
        var externalMovie = new ExternalMovie(
            1,
            "Test Movie",
            "Test Overview",
            ReleaseDate: new DateTime(2020, 1, 1),
            PosterPath: "/poster.jpg",
            BackdropPath: "/backdrop.jpg",
            TrailerUrl: "https://youtube.com/test"
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
        await ep.HandleAsync(new Query(1), CancellationToken.None);
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovie_Should_NotCache_When_NotFound()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.NotFound());

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(1), CancellationToken.None);
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
    
    
    [Test]
    public async Task GetMovie_Should_NotCache_When_Unauthorized()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Unauthorized());

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(1), CancellationToken.None);
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
    
    [Test]
    public async Task GetMovie_Should_NotCache_When_Service_Error()
    {
        // Arrange
        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Error());

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            _dbQueries);

        // Act
        await ep.HandleAsync(new Query(1), CancellationToken.None);
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
    
    
    [Test]
    public async Task GetMovie_Should_ReturnCachedResponse_On_SecondCall()
    {
        // Arrange
        var externalMovie = new ExternalMovie(
            ExternalId: 1,
            Title: "Test Movie",
            Overview: "Test Overview",
            ReleaseDate: new DateTime(2020, 1, 1),
            PosterPath: "/poster.jpg",
            BackdropPath: "/backdrop.jpg",
            TrailerUrl: "https://youtube.com/test"
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
        await ep.HandleAsync(new Query(Id: 1), CancellationToken.None);
        var firstResponse = ep.Response;

        await ep.HandleAsync(new Query(Id: 1), CancellationToken.None);
        var secondResponse = ep.Response;

        // Assert
        secondResponse.Should().BeEquivalentTo(firstResponse);  
    
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, "en", It.IsAny<CancellationToken>()),
            Times.Once);  
    }
    
    [Test]
    public async Task GetMovie_Should_CacheSeparately_Per_Language()
    {
        var externalMovie = new ExternalMovie(
            ExternalId: 1,
            Title: "Test Movie",
            Overview: "Test Overview",
            ReleaseDate: new DateTime(2020, 1, 1),
            PosterPath: "/poster.jpg",
            BackdropPath: "/backdrop.jpg",
            TrailerUrl: "https://youtube.com/test"
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
        await ep1.HandleAsync(new Query(1), CancellationToken.None);
        await ep1.HandleAsync(new Query(1), CancellationToken.None); 

        await ep2.HandleAsync(new Query(1), CancellationToken.None);
        await ep2.HandleAsync(new Query(1), CancellationToken.None);

        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task GetMovie_Should_NotCallExternalService_When_FoundInDb()
    {
        // Arrange 
        var movies = new List<Movie>
        {
            new() { ExternalId = 1, Year = 2020, PosterPath = "/poster.jpg" }
        };

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.BuildMockDbSet().Object);

        var dbQueries = new DbQueries(dbContextMock.Object);

        var ep = Factory.Create<Endpoint>(
            _externalMock.Object,
            _currentUserMock.Object,
            _cache,
            dbQueries);

        // Act
        await ep.HandleAsync(new Query(Id: 1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    
    [Test]
    public async Task GetMovie_Should_CallExternalService_When_NotFoundInDb()
    {
        // Arrange 
        var movies = new List<Movie>
        {
            new() { ExternalId = 1, Year = 2020, PosterPath = "/poster.jpg" }
        };

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.BuildMockDbSet().Object);

        var dbQueries = new DbQueries(dbContextMock.Object);
        
        var externalMovie = new ExternalMovie(
            ExternalId: 2,
            Title: "External Movie",
            Overview: "Overview",
            ReleaseDate: new DateTime(2021, 1, 1),
            PosterPath: "/poster2.jpg",
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
            dbQueries);

        // Act
        await ep.HandleAsync(new Query(Id: 2), CancellationToken.None);

        // Assert
        ep.Response.Id.Should().Be(2);
        _externalMock.Verify(
            x => x.GetMovieDetailsAsync(2, "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovie_Should_Publish_MovieReferencedEvent()
    {
        // Arrange
        var fakeHandler = new FakeMovieReferencedEventHandler();

        _externalMock
            .Setup(x => x.GetMovieDetailsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalMovie>.Success(
                new ExternalMovie(
                    ExternalId: 1,
                    Title: "Test Movie",
                    Overview: "Test Overview",
                    ReleaseDate: new DateTime(2020, 1, 1),
                    PosterPath: "/poster.jpg",
                    BackdropPath: "/backdrop.jpg",
                    TrailerUrl: "https://youtube.com/test"
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
        await ep.HandleAsync(new Query(1), CancellationToken.None);
        await Task.Delay(100);

        // Assert
        fakeHandler.WasHandled.Should().BeTrue();
        fakeHandler.MovieId.Should().Be(1);
    }

    private sealed class FakeMovieReferencedEventHandler : IEventHandler<MovieReferencedEvent>
    {
        public bool WasHandled { get; private set; }
        public int MovieId { get; private set; }
    
        public Task HandleAsync(MovieReferencedEvent e, CancellationToken c)
        {
            WasHandled = true;
            MovieId = e.ExternalId; 
        
            return Task.CompletedTask;
        }
    }
    
}