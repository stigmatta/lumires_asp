namespace Tests.ApiTests.Movies;

using FastEndpoints;
using FluentAssertions;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using lumires.Api.Features.Films.RateFilm;


public class RateMovieTests
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

        _dbContextMock.Setup(x => x.Films)
            .Returns(mock.Object);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private void SetupRatings(List<UserFilmRating> ratings)
    {
        var mock = ratings.BuildMockDbSet();

        _dbContextMock.Setup(x => x.UserFilmRatings)
            .Returns(mock.Object);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task RateMovie_Should_Return_404_When_Film_NotFound()
    {
        // Arrange
        SetupFilms([]);
        SetupRatings([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(999999, 4.5f),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task RateMovie_Should_Return_204_When_New_Rating_Added()
    {
        // Arrange
        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();

        SetupFilms([film]);
        SetupRatings([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, 4.5f),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);

        _dbContextMock.Verify(
            x => x.UserFilmRatings.Add(
                It.IsAny<UserFilmRating>()),
            Times.Once);
    }

    [Test]
    public async Task RateMovie_Should_Update_Existing_Rating()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId)
            .Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();

        var rating = new UserFilmRating(
            userId,
            film.Id,
            3.0f);

        SetupFilms([film]);
        SetupRatings([rating]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, 5.0f),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);

        rating.Rating.Should().Be(5.0f);
        rating.UpdatedAt.Should().NotBeNull();

        _dbContextMock.Verify(
            x => x.UserFilmRatings.Add(
                It.IsAny<UserFilmRating>()),
            Times.Never);
    }

    [Test]
    public async Task RateMovie_Should_SaveChanges()
    {
        // Arrange
        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();

        SetupFilms([film]);
        SetupRatings([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, 4.0f),
            CancellationToken.None);

        // Assert
        _dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task RateMovie_Should_Not_Create_New_Rating_When_Already_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId)
            .Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();

        var existingRating = new UserFilmRating(
            userId,
            film.Id,
            2.5f);

        SetupFilms([film]);
        SetupRatings([existingRating]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, 4.5f),
            CancellationToken.None);

        // Assert
        existingRating.Rating.Should().Be(4.5f);

        _dbContextMock.Verify(
            x => x.UserFilmRatings.Add(
                It.IsAny<UserFilmRating>()),
            Times.Never);
    }
}