using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Threads.UpdateThread;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Threads;

internal sealed class UpdateThreadTests
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

    private void SetupThreads(List<UserThread> threads)
    {
        _dbContextMock.Setup(x => x.Threads).Returns(threads.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task UpdateThread_Should_Return_404_When_Thread_Not_Found()
    {
        // Arrange
        SetupThreads([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(Guid.NewGuid(), "New Title", null, "New text"),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task UpdateThread_Should_Return_403_When_Not_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var thread = new UserThread(otherUserId, "Original Title", null, "Original text", true);
        SetupThreads([thread]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(thread.Id, "New Title", null, "New text"),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateThread_Should_Return_204_When_Owner_Updates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var thread = new UserThread(userId, "Original Title", null, "Original text", true);
        SetupThreads([thread]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(thread.Id, "New Title", null, "Updated text"),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.Threads.Update(thread), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateThread_Should_Apply_New_Text_And_Title()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var thread = new UserThread(userId, "Original Title", null, "Original text", true);
        SetupThreads([thread]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(thread.Id, "New Title", null, "Updated text", false),
            CancellationToken.None);

        // Assert
        thread.Title.Should().Be("New Title");
        thread.Text.Should().Be("Updated text");
        thread.IsSpoilerFree.Should().BeFalse();
    }
}
