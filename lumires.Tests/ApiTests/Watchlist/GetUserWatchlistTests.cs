using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Users.GetUserWatchlist;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Watchlist;

internal sealed class GetUserWatchlistTests
{
    private const string Username = "alice";

    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.LangCulture).Returns("en-US");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(
            _dataAccess,
            _currentUserMock.Object);
    }

    private void SetupData(List<User> users, List<Film> films, List<WatchlistFilm> watchlist)
    {
        _dbContextMock.Setup(x => x.Users).Returns(users.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Films).Returns(films.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.WatchlistFilms).Returns(watchlist.BuildMockDbSet().Object);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private static User PublicUser(Guid id)
    {
        return new User(id, Username, "alice@mail.com");
    }

    private static User PrivateUser(Guid id)
    {
        var user = new User(id, Username, "alice@mail.com");
        user.UserSettings.UpdatePrivacySettings(ProfileVisibility.Everyone, true, false, true, true);
        return user;
    }

    [Test]
    public async Task GetWatchlist_Should_Return_404_When_User_NotFound()
    {
        // Arrange
        SetupData([], [], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query { Username = Username }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task GetWatchlist_Should_Return_403_When_Private_And_Not_Owner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid()); // different user

        SetupData([PrivateUser(ownerId)], [], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query { Username = Username }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
    }

    [Test]
    public async Task GetWatchlist_Should_Return_200_With_Films_When_Public()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid()); // anonymous / other viewer

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        SetupData(
            [PublicUser(ownerId)],
            [film],
            [new WatchlistFilm(ownerId, film.Id)]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query { Username = Username }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().ContainSingle()
            .Which.Id.Should().Be(film.ExternalId);
        ep.Response.TotalResults.Should().Be(1);
    }

    [Test]
    public async Task GetWatchlist_Should_Return_200_When_Owner_Views_Own_Private_Watchlist()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(ownerId); // owner

        var film = Helpers.CreateFilmsWithVoteAverage([4.5f]).First();
        SetupData(
            [PrivateUser(ownerId)],
            [film],
            [new WatchlistFilm(ownerId, film.Id)]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query { Username = Username }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().ContainSingle()
            .Which.Id.Should().Be(film.ExternalId);
    }

    [Test]
    public async Task GetWatchlist_Should_Only_Return_Target_Users_Films()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(ownerId);

        var films = Helpers.CreateFilmsWithVoteAverage([4.5f, 3.0f]);
        var ownFilm = films[0];
        var otherFilm = films[1];

        SetupData(
            [PublicUser(ownerId)],
            films,
            [
                new WatchlistFilm(ownerId, ownFilm.Id),
                new WatchlistFilm(otherUserId, otherFilm.Id)
            ]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query { Username = Username }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().ContainSingle()
            .Which.Id.Should().Be(ownFilm.ExternalId);
    }
}
