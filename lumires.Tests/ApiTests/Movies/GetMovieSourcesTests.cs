using Ardalis.Result;
using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Movies.GetMovieSources;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Models;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace Tests.ApiTests.Movies;

internal sealed class GetMovieSourcesTests
{
    private FusionCache _cache = null!;
    private Mock<IStreamingService> _streamingMock = null!;


    [Before(Test)]
    public void Setup()
    {
        _cache = new FusionCache(new FusionCacheOptions());
        _streamingMock = new Mock<IStreamingService>();
    }

    [After(Test)]
    public void TearDown()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task GetMovieSources_Should_Be_404_When_EmptyList()
    {
        //Arrange
        _streamingMock.Setup(s => s.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result.NotFound());

        var ep = Factory.Create<Endpoint>(
            _streamingMock.Object);

        //Act
        await ep.HandleAsync(new Query(It.IsAny<int>()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);

        _streamingMock.Verify(
            x => x.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    [Arguments(1, "Netflix", "subscription", "https://netflix.com", "HD")]
    [Arguments(2, "Amazon Prime", "rent", "https://amazon.com", "4K")]
    [Arguments(3, "Disney+", "subscription", "https://disney.com", "SD")]
    public async Task GetMovieSources_Should_Be_200_And_MapData(
        int id,
        string name,
        string type,
        string url,
        string quality)
    {
        // Arrange
        var movieSource = new MovieSource(
            id,
            name,
            type,
            new Uri(url),
            quality,
            0);

        var sourceList = new List<MovieSource> { movieSource };

        _streamingMock
            .Setup(s => s.GetSourcesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success(sourceList));

        var ep = Factory.Create<Endpoint>(_streamingMock.Object);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);

        var firstResult = ep.Response.Sources.First();
        firstResult.ProviderName.Should().Be(name);
        firstResult.Type.Should().Be(type);
        firstResult.Url.Should().Be(url);

        _streamingMock.Verify(
            x => x.GetSourcesAsync(id, It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovieSources_Should_Be_500_When_ExternalServiceError()
    {
        //Arrange
        _streamingMock.Setup(s => s.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<List<MovieSource>>.Error());

        var ep = Factory.Create<Endpoint>(
            _streamingMock.Object);

        //Act
        await ep.HandleAsync(new Query(It.IsAny<int>()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(500);

        _streamingMock.Verify(
            x => x.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovieSources_Should_Be_401_When_Unauthorized()
    {
        //Arrange
        _streamingMock.Setup(s => s.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<List<MovieSource>>.Unauthorized());

        var ep = Factory.Create<Endpoint>(
            _streamingMock.Object);

        //Act
        await ep.HandleAsync(new Query(It.IsAny<int>()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(401);

        _streamingMock.Verify(
            x => x.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    [Arguments(1, "Netflix", "subscription", "https://netflix.com", "HD")]
    [Arguments(2, "Amazon Prime", "rent", "https://amazon.com", "4K")]
    [Arguments(3, "Disney+", "subscription", "https://disney.com", "SD")]
    public async Task GetMovieSources_Should_Return_Response_With_Sources(
        int id,
        string name,
        string type,
        string url,
        string quality)
    {
        // Arrange
        var expectedSources = new List<MovieSource>
        {
            new(
                id,
                name,
                type,
                new Uri(url),
                quality,
                0)
        };

        _streamingMock
            .Setup(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success(expectedSources));

        var endpoint = Factory.Create<Endpoint>(_streamingMock.Object);

        // Act
        await endpoint.HandleAsync(new Query(id), It.IsAny<CancellationToken>());

        // Assert
        endpoint.Response.Should().NotBeNull();
        endpoint.Response.Sources.Should().NotBeNull();
        endpoint.Response.Sources.Should().HaveCount(1);

        endpoint.Response.Sources[0].ProviderName.Should().Be(expectedSources[0].ProviderName);
    }


    [Test]
    public async Task GetMovie_Should_NotCache_When_NotFound()
    {
        // Arrange
        const int id = 1;
        _streamingMock.Setup(s => s.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<List<MovieSource>>.NotFound);

        var ep = Factory.Create<Endpoint>(_streamingMock.Object);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _streamingMock.Verify(
            x => x.GetSourcesAsync(id, It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task GetMovie_Should_NotCache_When_ExternalError()
    {
        // Arrange
        const int id = 1;
        _streamingMock.Setup(s => s.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<List<MovieSource>>.Error());

        var ep = Factory.Create<Endpoint>(_streamingMock.Object);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _streamingMock.Verify(
            x => x.GetSourcesAsync(id, It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task GetMovie_Should_NotCache_When_Unauthorized()
    {
        // Arrange
        const int id = 1;
        _streamingMock.Setup(s => s.GetSourcesAsync
                (It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<List<MovieSource>>.Unauthorized());

        var ep = Factory.Create<Endpoint>(_streamingMock.Object);

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _streamingMock.Verify(
            x => x.GetSourcesAsync(id, It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Exactly(2));
    }


    [Test]
    [Arguments(123, "US")]
    [Arguments(456, "GB")]
    public void CacheKeys_MovieSources_Should_Match_Expected_Format(int tmdbId, string region)
    {
        var formatted = CacheKeys.MovieSources(tmdbId, region);
        formatted.Should().Be($"sources:{tmdbId}:{region}");
    }

    [Test]
    [Arguments(123)]
    [Arguments(789)]
    public void CacheKeys_MovieSourceExternalId_Should_Match_Expected_Format(int tmdbId)
    {
        var formatted = CacheKeys.MovieSourceExternalId(tmdbId);
        formatted.Should().Be($"wm_id:{tmdbId}");
    }
}