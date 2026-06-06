using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.UnrateFilm;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Movies;

public class UnrateMovieTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            dataAccess ?? _dataAccess);
    }

    private void SetupFilms(List<Film> films)
    {
        var mock = films.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Films).Returns(mock.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private void SetupRatings(List<UserFilmRating> ratings)
    {
        var mock = ratings.BuildMockDbSet();
        _dbContextMock.Setup(x => x.UserFilmRatings).Returns(mock.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task UnrateMovie_Should_Return_404_When_Film_NotFound()
    {
        // Arrange
        SetupFilms([]);
        SetupRatings([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(999999),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task UnrateMovie_Should_Return_204_When_Rating_Not_Found()
    {
        // Arrange
        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();

        SetupFilms([film]);
        SetupRatings([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
    }

    [Test]
    public async Task UnrateMovie_Should_Return_204_And_Remove_Rating_When_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        var rating = new UserFilmRating(userId, film.Id, 4.5f);

        SetupFilms([film]);
        SetupRatings([rating]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);

        _dbContextMock.Verify(
            x => x.UserFilmRatings.Remove(It.Is<UserFilmRating>(r =>
                r.UserId == userId && r.FilmId == film.Id)),
            Times.Once);
    }

    [Test]
    public async Task UnrateMovie_Should_Not_Remove_Rating_Of_Another_User()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        var otherUserRating = new UserFilmRating(otherUserId, film.Id, 3.0f);

        SetupFilms([film]);
        SetupRatings([otherUserRating]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);

        _dbContextMock.Verify(
            x => x.UserFilmRatings.Remove(It.IsAny<UserFilmRating>()),
            Times.Never);
    }

    [Test]
    public async Task UnrateMovie_Should_SaveChanges_When_Rating_Removed()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        var rating = new UserFilmRating(userId, film.Id, 4.5f);

        SetupFilms([film]);
        SetupRatings([rating]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId),
            CancellationToken.None);

        // Assert
        _dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task UnrateMovie_Should_Not_SaveChanges_When_Rating_Not_Found()
    {
        // Arrange
        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();

        SetupFilms([film]);
        SetupRatings([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId),
            CancellationToken.None);

        // Assert
        _dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}