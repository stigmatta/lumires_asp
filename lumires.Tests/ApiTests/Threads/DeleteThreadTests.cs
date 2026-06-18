using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Threads.DeleteThread;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Auth;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Threads;

internal sealed class DeleteThreadTests
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
        _currentUserMock.Setup(x => x.UserRole).Returns(UserRoles.User);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);
    }

    private void SetupThreads(List<UserThread> threads)
    {
        _dbContextMock.Setup(x => x.Threads).Returns(threads.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task DeleteThread_Should_Return_204_When_Thread_Not_Found()
    {
        // Arrange
        SetupThreads([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(Guid.NewGuid()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteThread_Should_Return_204_And_Remove_When_Owner_Deletes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var thread = new UserThread(userId, "Title", null, "Thread text", true);
        SetupThreads([thread]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(thread.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.Threads.Remove(thread), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteThread_Should_Return_403_When_Not_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var thread = new UserThread(otherUserId, "Title", null, "Thread text", true);
        SetupThreads([thread]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(thread.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
        _dbContextMock.Verify(x => x.Threads.Remove(It.IsAny<UserThread>()), Times.Never);
    }

    [Test]
    public async Task DeleteThread_Should_Return_403_When_Admin_Tries_To_Delete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _currentUserMock.Setup(x => x.UserRole).Returns(UserRoles.Admin);

        var thread = new UserThread(userId, "Title", null, "Thread text", true);
        SetupThreads([thread]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(thread.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
    }
}
