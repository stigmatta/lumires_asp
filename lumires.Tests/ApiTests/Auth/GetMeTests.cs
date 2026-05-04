using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Auth.Queries.GetMe;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Auth;

internal sealed class GetMeEndpointTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _dbContextMock = new Mock<IAppDbContext>();
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task GetMe_Should_Return_200_With_CorrectData()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        const string expectedEmail = "morrigun0@gmail.com";
        const string expectedUsername = "testuser";

        _currentUserMock.Setup(x => x.UserId).Returns(expectedId);

        var users = new List<User>
        {
            new(expectedId, expectedUsername, expectedEmail)
        }.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Users).Returns(users.Object);

        var ep = Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.Should().NotBeNull();
        ep.Response.Id.Should().Be(expectedId);
        ep.Response.Email.Should().Be(expectedEmail);
        ep.Response.Username.Should().Be(expectedUsername);
        ep.Response.AvatarUrl.Should().BeNull();
    }

    [Test]
    public async Task GetMe_Should_Return_403_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var users = new List<User>().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(users.Object);

        var ep = Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
    }

    [Test]
    public async Task GetMe_Should_Return_403_When_UserIdIsEmpty()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.Empty);

        var users = new List<User>().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(users.Object);

        var ep = Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
    }

    [Test]
    public async Task GetMe_Should_Return_AvatarUrl_When_Present()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string avatarUrl = "/avatars/123.jpg";

        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var users = new List<User>
        {
            new(userId, "user", "test@example.com")
        }.BuildMockDbSet();

        users.Object.First().SetAvatarUrl(avatarUrl);


        _dbContextMock.Setup(x => x.Users).Returns(users.Object);

        var ep = Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.AvatarUrl.Should().Be(avatarUrl);
    }
}