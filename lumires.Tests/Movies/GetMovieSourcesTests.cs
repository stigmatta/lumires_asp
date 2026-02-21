using System.Net;
using Api.Features.Movies.GetMovieSources;
using Ardalis.Result;
using Core.Abstractions.Services;
using Core.Constants;
using Core.Models;
using FastEndpoints;
using FluentAssertions;
using Infrastructure.Services.Watchmode;
using Moq;
using Refit;
using ZiggyCreatures.Caching.Fusion;

namespace lumires.Tests.Movies;

internal sealed class GetMovieSourcesTests
{
    private FusionCache _cache = null!;
    private Mock<IStreamingService> _streamingMock = null!;
    private Mock<IWatchmodeApi> _watchmodeApi = null!;


    [Before(Test)]
    public void Setup()
    {
        _cache = new FusionCache(new FusionCacheOptions());
        _streamingMock = new Mock<IStreamingService>();
        _watchmodeApi = new Mock<IWatchmodeApi>();
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
    [Arguments(1, "Netflix", "subscription", "https://netflix.com", "HD")]
    [Arguments(2, "Amazon Prime", "rent", "https://amazon.com", "4K")]
    [Arguments(3, "Disney+", "subscription", "https://disney.com", "SD")]
    public async Task GetMovieSources_Should_Cache_Successful_Result(
        int id,
        string name,
        string type,
        string url,
        string quality)
    {
        //Arrange
        const int tmdbId = 1;

        var mockSearchResponse = new WatchmodeSearchResponse(
            new List<WatchmodeTitleResult>
            {
                new(12345, "Test Movie", tmdbId, "movie")
            }
        );

        var mockSourcesResponse = new List<WatchmodeSourceResponse>
        {
            new(
                name,
                type,
                new Uri(url),
                quality,
                null
            )
        };

        _watchmodeApi
            .Setup(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new ApiResponse<WatchmodeSearchResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSearchResponse,
                new RefitSettings()));

        _watchmodeApi
            .Setup(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<WatchmodeSourceResponse>>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSourcesResponse,
                new RefitSettings()));

        var cache = new FusionCache(new FusionCacheOptions());
        var service = new WatchmodeService(_watchmodeApi.Object, cache);

        //Act
        await service.GetSourcesAsync(tmdbId, CancellationToken.None);
        await service.GetSourcesAsync(tmdbId, CancellationToken.None);

        //Assert
        _watchmodeApi.Verify(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()),
            Times.Once);

        _watchmodeApi.Verify(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
    public async Task GetMovie_Should_NotCache_When_ResultZero()
    {
        //Arrange
        const int id = 1;

        var mockSearchResponse = new WatchmodeSearchResponse(
            new List<WatchmodeTitleResult>()
        );

        _watchmodeApi
            .Setup(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new ApiResponse<WatchmodeSearchResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSearchResponse,
                new RefitSettings()));

        var cache = new FusionCache(new FusionCacheOptions());
        var service = new WatchmodeService(_watchmodeApi.Object, cache);

        // Act
        await service.GetSourcesAsync(id, CancellationToken.None);
        await service.GetSourcesAsync(id, CancellationToken.None);

        //Assert
        _watchmodeApi.Verify(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()),
            Times.Exactly(2));

        _watchmodeApi.Verify(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [Arguments(1, "Netflix", "subscription", "https://netflix.com", "HD")]
    [Arguments(2, "Amazon Prime", "rent", "https://amazon.com", "4K")]
    [Arguments(3, "Disney+", "subscription", "https://disney.com", "SD")]
    public async Task WatchmodeService_Should_Cache_Successful_Result(
        int id,
        string name,
        string type,
        string url,
        string quality)
    {
        //Arrange
        const int tmdbId = 1;

        var mockSearchResponse = new WatchmodeSearchResponse(
            new List<WatchmodeTitleResult>
            {
                new(12345, "Test Movie", tmdbId, "movie")
            }
        );

        var mockSourcesResponse = new List<WatchmodeSourceResponse>
        {
            new(
                name,
                type,
                new Uri(url),
                quality,
                null
            )
        };

        _watchmodeApi
            .Setup(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApiResponse<WatchmodeSearchResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSearchResponse,
                new RefitSettings()));

        _watchmodeApi
            .Setup(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<WatchmodeSourceResponse>>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSourcesResponse,
                new RefitSettings()));

        var cache = new FusionCache(new FusionCacheOptions());
        var service = new WatchmodeService(_watchmodeApi.Object, cache);

        //Act
        var result1 = await service.GetSourcesAsync(tmdbId, CancellationToken.None);
        var result2 = await service.GetSourcesAsync(tmdbId, CancellationToken.None);

        //Assert
        result1.Should().BeEquivalentTo(result2);

        _watchmodeApi.Verify(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()),
            Times.Once);

        _watchmodeApi.Verify(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    [Arguments(1, "Netflix", "subscription", "https://netflix.com", "HD")]
    [Arguments(2, "Amazon Prime", "rent", "https://amazon.com", "4K")]
    [Arguments(3, "Disney+", "subscription", "https://disney.com", "SD")]
    public async Task GetSourcesAsync_Should_Use_Correct_Cache_Key_For_Sources(
        int id,
        string name,
        string type,
        string url,
        string quality)
    {
        // Arrange
        const int tmdbId = 123;
        const string region = "US";
        var expectedCacheKey = CacheKeys.MovieSources(tmdbId, region);

        var mockSearchResponse = new WatchmodeSearchResponse(
            new List<WatchmodeTitleResult>
            {
                new(12345, "Test Movie", tmdbId, "movie")
            }
        );

        var mockSourcesResponse = new List<WatchmodeSourceResponse>
        {
            new(
                name,
                type,
                new Uri(url),
                quality,
                null
            )
        };

        _watchmodeApi
            .Setup(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApiResponse<WatchmodeSearchResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSearchResponse,
                new RefitSettings()));

        _watchmodeApi
            .Setup(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<WatchmodeSourceResponse>>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSourcesResponse,
                new RefitSettings()));


        var service = new WatchmodeService(_watchmodeApi.Object, _cache);

        // Act
        await service.GetSourcesAsync(tmdbId, CancellationToken.None);

        // Assert
        var cachedResult = await _cache.GetOrDefaultAsync<List<MovieSource>>(expectedCacheKey);
        cachedResult.Should().NotBeNull();
        cachedResult.Should().HaveCount(1);

        cachedResult[0].ProviderName.Should().Be(name);
        cachedResult[0].ExternalId.Should().Be(tmdbId);
    }

    [Test]
    public async Task GetWatchmodeIdAsync_Should_Cache_With_Correct_Key()
    {
        // Arrange
        const int tmdbId = 456;
        const int expectedWatchmodeId = 67890;
        var expectedCacheKey = CacheKeys.MovieSourceExternalId(tmdbId);

        var mockSearchResponse = new WatchmodeSearchResponse(
            new List<WatchmodeTitleResult>
            {
                new(expectedWatchmodeId, "Test Movie", tmdbId, "movie")
            }
        );

        _watchmodeApi
            .Setup(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApiResponse<WatchmodeSearchResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSearchResponse,
                new RefitSettings()));

        _watchmodeApi
            .Setup(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<WatchmodeSourceResponse>>(
                new HttpResponseMessage(HttpStatusCode.OK),
                [],
                new RefitSettings()));

        var cache = new FusionCache(new FusionCacheOptions());
        var service = new WatchmodeService(_watchmodeApi.Object, cache);

        // Act
        await service.GetSourcesAsync(tmdbId, CancellationToken.None);

        // Assert
        var cachedId = await cache.GetOrDefaultAsync<int>(expectedCacheKey);

        cachedId.Should().Be(expectedWatchmodeId);
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


    [Test]
    [Arguments(1, "Netflix", "subscription", "https://netflix.com", "HD")]
    [Arguments(2, "Amazon Prime", "rent", "https://amazon.com", "4K")]
    [Arguments(3, "Disney+", "subscription", "https://disney.com", "SD")]
    public async Task GetMovieSources_Should_Call_GetSourcesAsync_If_IdNotZero(
        int id,
        string name,
        string type,
        string url,
        string quality)
    {
        // Arrange
        const int tmdbId = 123;
        const int watchmodeId = 12345;

        var mockSearchResponse = new WatchmodeSearchResponse(
            new List<WatchmodeTitleResult>
            {
                new(watchmodeId, "Test Movie", tmdbId, "movie")
            }
        );

        var mockSourcesResponse = new List<WatchmodeSourceResponse>
        {
            new(
                name,
                type,
                new Uri(url),
                quality,
                null
            )
        };

        _watchmodeApi
            .Setup(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApiResponse<WatchmodeSearchResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSearchResponse,
                new RefitSettings()));

        _watchmodeApi
            .Setup(x => x.GetSourcesAsync(
                watchmodeId,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<WatchmodeSourceResponse>>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSourcesResponse,
                new RefitSettings()));

        var service = new WatchmodeService(_watchmodeApi.Object, _cache);

        // Act
        var result = await service.GetSourcesAsync(tmdbId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ProviderName.Should().Be(name);

        _watchmodeApi.Verify(x => x.GetSourcesAsync(
                watchmodeId,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetMovieSources_Should_NotCall_GetSourcesAsync_If_WatchmodeId_IsZero()
    {
        // Arrange
        const int tmdbId = 456;

        var mockSearchResponse = new WatchmodeSearchResponse(
            new List<WatchmodeTitleResult>()
        );

        _watchmodeApi
            .Setup(x => x.SearchByTmdbIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ApiResponse<WatchmodeSearchResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                mockSearchResponse,
                new RefitSettings()));

        var service = new WatchmodeService(_watchmodeApi.Object, _cache);

        // Act
        var result = await service.GetSourcesAsync(tmdbId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _watchmodeApi.Verify(x => x.GetSourcesAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}