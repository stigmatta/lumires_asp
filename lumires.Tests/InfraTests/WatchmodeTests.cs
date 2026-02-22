using System.Net;
using FluentAssertions;
using Infrastructure.Services.Watchmode;
using lumires.Core.Constants;
using lumires.Core.Models;
using Moq;
using Refit;
using ZiggyCreatures.Caching.Fusion;

namespace Tests.InfraTests;

internal sealed class WatchmodeTests
{
    private FusionCache _cache = null!;
    private Mock<IWatchmodeApi> _watchmodeApi = null!;


    [Before(Test)]
    public void Setup()
    {
        _cache = new FusionCache(new FusionCacheOptions());
        _watchmodeApi = new Mock<IWatchmodeApi>();
    }

    [After(Test)]
    public void TearDown()
    {
        _cache.Dispose();
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
    public async Task WatchmodeService_Should_NotCache_When_ResultZero()
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