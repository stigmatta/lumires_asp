using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.WatchFilm;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Movies;

internal sealed class WatchFilmTests
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

    private void SetupData(List<Film> films, List<WatchedFilm> watched)
    {
        _dbContextMock.Setup(x => x.Films).Returns(films.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.WatchedFilms).Returns(watched.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task Watch_Should_Return_404_When_Film_NotFound()
    {
        // Arrange
        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(999999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
        _dbContextMock.Verify(x => x.WatchedFilms.Add(It.IsAny<WatchedFilm>()), Times.Never);
        _dbContextMock.Verify(x => x.WatchedFilms.Remove(It.IsAny<WatchedFilm>()), Times.Never);
    }

    [Test]
    public async Task Watch_Should_Add_And_Return_IsWatched_True_When_Not_Watched()
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
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.IsWatched.Should().BeTrue();

        _dbContextMock.Verify(
            x => x.WatchedFilms.Add(It.Is<WatchedFilm>(w => w.UserId == userId && w.FilmId == film.Id)),
            Times.Once);
        _dbContextMock.Verify(x => x.WatchedFilms.Remove(It.IsAny<WatchedFilm>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Watch_Should_Remove_And_Return_IsWatched_False_When_Already_Watched()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        var watched = new WatchedFilm(userId, film.Id);
        SetupData([film], [watched]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.IsWatched.Should().BeFalse();

        _dbContextMock.Verify(x => x.WatchedFilms.Remove(watched), Times.Once);
        _dbContextMock.Verify(x => x.WatchedFilms.Add(It.IsAny<WatchedFilm>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
