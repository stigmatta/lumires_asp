using System.Reflection;
using FluentAssertions;
using lumires.Api.Features.Films.GetFilm;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Movies;

internal sealed class GetFilmTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.LangCulture).Returns("en-US");
    }

    private DataAccess CreateDataAccess()
    {
        return new DataAccess(_dbContextMock.Object, _currentUserMock.Object);
    }

    private void SetupData(List<Film> films, List<WatchedFilm> watched)
    {
        _dbContextMock.Setup(x => x.Films).Returns(films.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.WatchedFilms).Returns(watched.BuildMockDbSet().Object);
    }

    private static void AddUserRating(Film film, Guid userId, float rating)
    {
        var entry = new UserFilmRating(userId, film.Id, rating);
        var field = typeof(Film).GetField("_userRatings", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var ratings = (List<UserFilmRating>)field.GetValue(film)!;
        ratings.Add(entry);
    }

    [Test]
    public async Task GetFilm_Should_Return_MyRating_For_Current_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        AddUserRating(film, userId, 3.5f);
        AddUserRating(film, Guid.NewGuid(), 1.0f);

        SetupData([film], []);
        var sut = CreateDataAccess();

        // Act
        var response = await sut.GetFilmByIdAsync(film.ExternalId, "en-US", CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response!.MyRating.Should().Be(3.5f);
    }

    [Test]
    public async Task GetFilm_Should_Return_Null_MyRating_When_User_Has_Not_Rated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        AddUserRating(film, Guid.NewGuid(), 5.0f);

        SetupData([film], []);
        var sut = CreateDataAccess();

        // Act
        var response = await sut.GetFilmByIdAsync(film.ExternalId, "en-US", CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response!.MyRating.Should().BeNull();
    }

    [Test]
    public async Task GetFilm_Should_Return_Null_MyRating_When_Anonymous()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.Empty);

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        AddUserRating(film, Guid.NewGuid(), 5.0f);

        SetupData([film], []);
        var sut = CreateDataAccess();

        // Act
        var response = await sut.GetFilmByIdAsync(film.ExternalId, "en-US", CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response!.MyRating.Should().BeNull();
    }
}
