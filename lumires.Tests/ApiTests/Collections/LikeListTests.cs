using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.LikeFilmsList;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

public class LikeListTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;

    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<INotificationService> _notificationMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _notificationMock = new Mock<INotificationService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _dataAccess = new DataAccess(_dbContextMock.Object, _currentUserMock.Object, _notificationMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        return Factory.Create<Endpoint>(dataAccess ?? _dataAccess);
    }

    private void SetupLists(List<FilmsList> filmsLists)
    {
        var userId = _currentUserMock.Object.UserId;
        var user = new User(userId, "testuser", "test@test.com");

        foreach (var list in filmsLists)
            typeof(FilmsList).GetProperty("User")!.SetValue(list, user);

        _dbContextMock.Setup(x => x.FilmsLists).Returns(filmsLists.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Users).Returns(new List<User> { user }.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object, _currentUserMock.Object, _notificationMock.Object);
    }

    [Test]
    public async Task Should_Return_404_When_List_NotFound()
    {
        // Arrange
        SetupLists([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(Guid.NewGuid()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task Should_Return_200_When_Liked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var filmsList = new FilmsList("Title", userId, "Text");
        SetupLists([filmsList]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(filmsList.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task Should_Return_IsLiked_True_On_First_Like()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var filmsList = new FilmsList("Title", userId, "Text");
        SetupLists([filmsList]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(filmsList.Id), CancellationToken.None);

        // Assert
        ep.Response.IsLiked.Should().BeTrue();
        ep.Response.LikesCount.Should().Be(1);
    }

    [Test]
    public async Task LikeReview_Should_Toggle_Like_Off_On_Second_Call()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var filmsList = new FilmsList("Title", userId, "Text");
        filmsList.ToggleLike(userId);
        SetupLists([filmsList]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(filmsList.Id), CancellationToken.None);

        // Assert
        ep.Response.IsLiked.Should().BeFalse();
        ep.Response.LikesCount.Should().Be(0);
    }

    [Test]
    public async Task LikeReview_Should_IncrementLikesCount_When_Liked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var filmsList = new FilmsList("Title", userId, "Text");
        SetupLists([filmsList]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(filmsList.Id), CancellationToken.None);

        // Assert
        ep.Response.LikesCount.Should().Be(1);
    }
}