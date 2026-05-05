using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Genres.GetGenres;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace Tests.ApiTests.Genres;

internal sealed class GetGenresTests
{
    private FusionCache _cache = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.LangCulture)
            .Returns("en");

        _cache = new FusionCache(new FusionCacheOptions());

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.Genres)
            .Returns(new List<Genre>().BuildMockDbSet().Object);

        _dbContextMock
            .Setup(x => x.GenreLocalizations)
            .Returns(new List<GenreLocalization>().BuildMockDbSet().Object);
    }

    [After(Test)]
    public void TearDown()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task GetGenres_Should_Be_200_When_Empty()
    {
        //Arrange

        var dbQueries = new DataAccess(_dbContextMock.Object);

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
    public async Task GetGenres_Should_Return_200_And_Data_When_Not_Empty()
    {
        // Arrange
        var genre = new Genre(1);
        genre.AddLocalization("Action", "en");
        genre.AddLocalization("Екшн", "uk");

        var db = new Mock<IAppDbContext>();

        db.Setup(x => x.Genres)
            .Returns(new List<Genre> { genre }.BuildMockDbSet().Object);

        var dbQueries = new DataAccess(db.Object);

        var ep = Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _cache,
            dbQueries);

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);

        ep.Response.Genres.Should().NotBeNull();
        ep.Response.Genres.Count.Should().Be(1);

        var result = ep.Response.Genres.First();
        result.Id.Should().Be(genre.Id);
        result.Name.Should().Be("Action");
    }
}