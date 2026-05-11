using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Movies.GetThisWeekPopular;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace Tests.ApiTests.Movies;

internal sealed class GetThisWeekRecentReleasesTests
{
    private FusionCache _cache = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.LangCulture)
            .Returns("en");

        _cache = new FusionCache(new FusionCacheOptions());

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(new List<Movie>().BuildMockDbSet().Object);
    }

    [After(Test)]
    public void TearDown()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task GetThisWeekRecentReleases_Should_Be_200_When_Empty()
    {
        //Arrange
        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(new List<Movie>().BuildMockDbSet().Object);

        var dbQueries = new DataAccess(dbContextMock.Object);

        var ep = Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _cache,
            dbQueries);

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
    }

    [Test]
    [Arguments(1, "2010-07-16", "/poster1.jpg", 4.5, 200, 20, 100, "Warner")]
    [Arguments(42, "2014-11-07", "/poster2.jpg", 3.8, 350, 20, 200, "HBO")]
    public async Task GetThisWeekRecentReleases_Should_Be_200_And_Correct_When_Not_Empty(
        int externalId,
        string dateStr,
        string posterPath,
        float voteAverage,
        int voteCount,
        float popularity,
        int runtime,
        string company)

    {
        //Arrange
        var releaseDate = DateOnly.Parse(dateStr);

        var movies = new List<Movie>
        {
            new(externalId, releaseDate, posterPath, voteAverage, voteCount, popularity, runtime, company)
        }.BuildMockDbSet();

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.Object);

        var dbQueries = new DataAccess(dbContextMock.Object);

        var ep = Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _cache,
            dbQueries);

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Items[0].ExternalId.Should().Be(externalId);
        ep.Response.Items[0].VoteCount.Should().Be(voteCount);
    }


    [Test]
    [Arguments(1, "2010-07-16", "/poster1.jpg", 4.5, 200, 20, 100, "Warner")]
    [Arguments(42, "2014-11-07", "/poster2.jpg", 3.8, 350, 20, 200, "HBO")]
    public async Task GetThisWeekRecentReleases_Should_ReturnCachedResponse_On_SecondCall(
        int externalId,
        string dateStr,
        string posterPath,
        float voteAverage,
        int voteCount,
        float popularity,
        int runtime,
        string company)

    {
        //Arrange
        var releaseDate = DateOnly.Parse(dateStr);

        var movies = new List<Movie>
        {
            new(externalId, releaseDate, posterPath, voteAverage, voteCount, popularity, runtime, company)
        }.BuildMockDbSet();

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.Object);

        var dbQueries = new DataAccess(dbContextMock.Object);

        var ep = Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _cache,
            dbQueries);

        // Act
        await ep.HandleAsync(CancellationToken.None);
        var firstResponse = ep.Response;

        await ep.HandleAsync(CancellationToken.None);
        var secondResponse = ep.Response;

        // Assert
        secondResponse.Should().BeEquivalentTo(firstResponse);
    }

    [Test]
    [Arguments(1, "2010-07-16", "/poster1.jpg", 4.5, 200, 20, 100, "Warner")]
    [Arguments(42, "2014-11-07", "/poster2.jpg", 3.8, 350, 20, 200, "HBO")]
    public async Task GetThisWeekRecentReleases_Should_CacheSeparately_Per_Language(
        int externalId,
        string dateStr,
        string posterPath,
        float voteAverage,
        int voteCount,
        float popularity,
        int runtime,
        string company)
    {
        //Arrange
        var releaseDate = DateOnly.Parse(dateStr);

        var movies = new List<Movie>
        {
            new(externalId, releaseDate, posterPath, voteAverage, voteCount, popularity, runtime, company)
        }.BuildMockDbSet();

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(movies.Object);

        var dbQueries = new DataAccess(dbContextMock.Object);
        var enUserMock = new Mock<ICurrentUserService>();
        enUserMock.Setup(x => x.LangCulture).Returns("en");

        var uaUserMock = new Mock<ICurrentUserService>();
        uaUserMock.Setup(x => x.LangCulture).Returns("uk-UA");

        var ep1 = Factory.Create<Endpoint>(
            enUserMock.Object, _cache, dbQueries);

        var ep2 = Factory.Create<Endpoint>(
            uaUserMock.Object, _cache, dbQueries);

        // Act
        await ep1.HandleAsync(CancellationToken.None);
        var ep1FirstResponse = ep1.Response;

        await ep1.HandleAsync(CancellationToken.None);
        var ep1SecondResponse = ep1.Response;

        await ep2.HandleAsync(CancellationToken.None);
        var ep2FirstResponse = ep2.Response;

        await ep2.HandleAsync(CancellationToken.None);
        var ep2SecondResponse = ep2.Response;

        // Assert
        ep1FirstResponse.Items.Should().BeEquivalentTo(ep1SecondResponse.Items);
        ep2FirstResponse.Items.Should().BeEquivalentTo(ep2SecondResponse.Items);

        dbContextMock.Verify(x => x.Movies, Times.Exactly(2));
    }

    [Test]
    public async Task GetThisWeekRecentReleases_Should_ReturnCachedData_WhenDataAccessFails_FailSafe()
    {
        // Arrange
        var callCount = 0;
        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Movies)
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    return new List<Movie>
                            { new(1, DateOnly.Parse("2010-07-16"), "/poster.jpg", 4.5f, 200, 20f, 200, "HBO") }
                        .BuildMockDbSet().Object;

                throw new Exception("DB недоступна");
            });

        var dbQueries = new DataAccess(dbContextMock.Object);
        var userMock = new Mock<ICurrentUserService>();
        userMock.Setup(x => x.LangCulture).Returns("en");

        var ep = Factory.Create<Endpoint>(userMock.Object, _cache, dbQueries);

        // Act
        await ep.HandleAsync(CancellationToken.None);
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task GetThisWeekRecentReleases_Should_ReadLangCulture_OnEveryRequest()
    {
        // Arrange
        var movies = new List<Movie>().BuildMockDbSet();
        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock.Setup(x => x.Movies).Returns(movies.Object);

        var userMock = new Mock<ICurrentUserService>();
        userMock.Setup(x => x.LangCulture).Returns("en");

        var ep = Factory.Create<Endpoint>(userMock.Object, _cache, new DataAccess(dbContextMock.Object));

        // Act
        await ep.HandleAsync(CancellationToken.None);
        await ep.HandleAsync(CancellationToken.None);
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        userMock.Verify(x => x.LangCulture, Times.Exactly(3));
    }

    [Test]
    public async Task GetThisWeekRecentReleases_Should_ReturnEmptyResponse_WhenDataAccessReturnsNull()
    {
        // Arrange
        var movies = new List<Movie>().BuildMockDbSet();
        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock.Setup(x => x.Movies).Returns(movies.Object);

        var userMock = new Mock<ICurrentUserService>();
        userMock.Setup(x => x.LangCulture).Returns("en");

        var ep = Factory.Create<Endpoint>(userMock.Object, _cache, new DataAccess(dbContextMock.Object));

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.Should().NotBeNull();
        ep.Response.Items.Should().BeEmpty();
    }
}