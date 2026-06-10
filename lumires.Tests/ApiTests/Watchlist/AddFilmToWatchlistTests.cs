using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.AddFilmToWatchlist;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Watchlist;

internal sealed class AddFilmToWatchlistTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<IFilmResolver> _filmResolverMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.LangCulture).Returns("en-US");

        _filmResolverMock = new Mock<IFilmResolver>();
        _filmResolverMock
            .Setup(x => x.EnsureFilmExistsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _dataAccess,
            _filmResolverMock.Object);
    }

    private void SetupData(List<Film> films, List<WatchlistFilm> watchlist)
    {
        _dbContextMock.Setup(x => x.Films).Returns(films.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.WatchlistFilms).Returns(watchlist.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private static WatchlistFilm WatchlistEntry(Guid userId, Film film)
    {
        var entry = new WatchlistFilm(userId, film.Id);
        typeof(WatchlistFilm).GetProperty(nameof(WatchlistFilm.Film))!.SetValue(entry, film);
        return entry;
    }

    [Test]
    public async Task AddToWatchlist_Should_Return_404_When_Film_NotFound()
    {
        // Arrange
        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(999999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
        _dbContextMock.Verify(x => x.WatchlistFilms.Add(It.IsAny<WatchlistFilm>()), Times.Never);
    }

    [Test]
    public async Task AddToWatchlist_Should_Return_204_And_Add_When_New()
    {
        // Arrange
        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        SetupData([film], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.WatchlistFilms.Add(It.IsAny<WatchlistFilm>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AddToWatchlist_Should_Not_Add_When_Already_In_Watchlist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        SetupData([film], [WatchlistEntry(userId, film)]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.WatchlistFilms.Add(It.IsAny<WatchlistFilm>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AddToWatchlist_Should_Add_Entry_For_Current_User_And_Film()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        SetupData([film], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        _dbContextMock.Verify(
            x => x.WatchlistFilms.Add(It.Is<WatchlistFilm>(w =>
                w.UserId == userId && w.FilmId == film.Id)),
            Times.Once);
    }
}
