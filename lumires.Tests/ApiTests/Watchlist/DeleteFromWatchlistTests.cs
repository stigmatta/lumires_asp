using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.DeleteFromWatchlist;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Watchlist;

internal sealed class DeleteFromWatchlistTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _dataAccess);
    }

    private void SetupWatchlist(List<WatchlistFilm> watchlist)
    {
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
    public async Task DeleteFromWatchlist_Should_Return_204_When_Not_In_Watchlist()
    {
        // Arrange
        SetupWatchlist([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(999999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.WatchlistFilms.Remove(It.IsAny<WatchlistFilm>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteFromWatchlist_Should_Return_204_And_Remove_When_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        SetupWatchlist([WatchlistEntry(userId, film)]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(
            x => x.WatchlistFilms.Remove(It.Is<WatchlistFilm>(w =>
                w.UserId == userId && w.FilmId == film.Id)),
            Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteFromWatchlist_Should_Not_Remove_Entry_Of_Another_User()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        SetupWatchlist([WatchlistEntry(otherUserId, film)]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.WatchlistFilms.Remove(It.IsAny<WatchlistFilm>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
