using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Relationships.UnfollowUser;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Relationships;

internal sealed class UnfollowUserTests
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

    private void SetupData(List<UsersRelationship> relationships)
    {
        _dbContextMock.Setup(x => x.Relationships).Returns(relationships.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task UnfollowUser_Should_Return_204_When_Unfollowing_Self()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        SetupData([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(userId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UnfollowUser_Should_Return_204_When_No_Follow_Relationship_Exists()
    {
        // Arrange
        SetupData([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(Guid.NewGuid()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UnfollowUser_Should_Return_204_And_Remove_When_Follow_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var relationship = new UsersRelationship(
            userId,
            targetId,
            UserRelationshipType.Follow,
            UserRelationshipStatus.Accepted);

        SetupData([relationship]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(targetId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.Relationships.Remove(relationship), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UnfollowUser_Should_Not_Remove_Follow_Of_Wrong_Direction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var reverseRelationship = new UsersRelationship(
            targetId,
            userId,
            UserRelationshipType.Follow,
            UserRelationshipStatus.Accepted);

        SetupData([reverseRelationship]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(targetId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.Relationships.Remove(It.IsAny<UsersRelationship>()), Times.Never);
    }
}
