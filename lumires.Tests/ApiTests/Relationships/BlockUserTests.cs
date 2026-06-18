using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Relationships.BlockUser;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Relationships;

internal sealed class BlockUserTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private DataAccess _dataAccess = null!;

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
        return Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);
    }

    private void SetupData(List<User> users, List<UsersRelationship> relationships)
    {
        _dbContextMock.Setup(x => x.Users).Returns(users.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Relationships).Returns(relationships.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task BlockUser_Should_Return_204_When_Blocking_Self()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(userId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BlockUser_Should_Return_404_When_Target_User_Not_Found()
    {
        // Arrange
        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(Guid.NewGuid()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task BlockUser_Should_Return_204_When_Block_Already_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetUser = new User(Guid.NewGuid(), "target", "target@mail.com");
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var blockRelationship = new UsersRelationship(
            userId,
            targetUser.Id,
            UserRelationshipType.Block,
            UserRelationshipStatus.Accepted);

        SetupData([targetUser], [blockRelationship]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(targetUser.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task BlockUser_Should_Return_204_And_Add_Block_When_No_Relationship()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetUser = new User(Guid.NewGuid(), "target", "target@mail.com");
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        SetupData([targetUser], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(targetUser.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.Relationships.Add(It.IsAny<UsersRelationship>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task BlockUser_Should_Update_Existing_Follow_To_Block()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetUser = new User(Guid.NewGuid(), "target", "target@mail.com");
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var followRelationship = new UsersRelationship(
            userId,
            targetUser.Id,
            UserRelationshipType.Follow,
            UserRelationshipStatus.Accepted);

        SetupData([targetUser], [followRelationship]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(targetUser.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.Relationships.Update(followRelationship), Times.Once);
        followRelationship.Type.Should().Be(UserRelationshipType.Block);
    }
}
