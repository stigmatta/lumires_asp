using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Threads.CreateThreadComment;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Threads;

internal sealed class CreateThreadCommentTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<INotificationService> _notificationMock = null!;
    private Mock<IStringLocalizer<SharedResource>> _localizerMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _notificationMock = new Mock<INotificationService>();
        _localizerMock = new Mock<IStringLocalizer<SharedResource>>();

        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _dbContextMock
            .Setup(x => x.ThreadComments)
            .Returns(new List<UserThreadComment>().BuildMockDbSet().Object);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationMock
            .Setup(x => x.SendToUsers(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<NotificationMessage>()));

        _dataAccess = new DataAccess(_dbContextMock.Object, _notificationMock.Object, _currentUserMock.Object, _localizerMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        var da = dataAccess ?? _dataAccess;
        return Factory.Create<Endpoint>(
            ctx =>
            {
                var services = new ServiceCollection();
                services.AddSingleton(_currentUserMock.Object);
                services.AddSingleton(_notificationMock.Object);
                services.AddSingleton(da);
                services.AddSingleton(Mock.Of<LinkGenerator>());
                services.AddRouting();
                ctx.RequestServices = services.BuildServiceProvider();
            },
            da);
    }

    private void SetupThreads(List<UserThread> threads, List<User>? extraUsers = null)
    {
        var currentUserId = _currentUserMock.Object.UserId;
        var currentUser = new User(currentUserId, "currentuser", "current@test.com");

        foreach (var thread in threads)
            thread.SetUser(currentUser);

        var allUsers = new List<User> { currentUser };
        if (extraUsers != null) allUsers.AddRange(extraUsers);

        _dbContextMock.Setup(x => x.Threads).Returns(threads.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Users).Returns(allUsers.BuildMockDbSet().Object);
        _dataAccess = new DataAccess(_dbContextMock.Object, _notificationMock.Object, _currentUserMock.Object, _localizerMock.Object);
    }

    [Test]
    public async Task Should_Be_201_When_Successfully_Created()
    {
        var thread = new UserThread(Guid.NewGuid(), null, null, "Review text", false);
        SetupThreads([thread]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(thread.Id, "Great review!", null),
            CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task Should_Return_Correct_Response()
    {
        var thread = new UserThread(Guid.NewGuid(), null, null, "Review text", false);
        SetupThreads([thread]);

        var ep = CreateEndpoint();
        var command = new Command(thread.Id, "Great review!", null);

        await ep.HandleAsync(command, CancellationToken.None);

        ep.Response.Text.Should().Be(command.Text);
        ep.Response.Id.Should().NotBe(Guid.Empty);
        ep.Response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Should_Be_404_When_Thread_NotFound()
    {
        SetupThreads([]);
        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(Guid.NewGuid(), "Some comment", null),
            CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task Should_Send_Notification_To_ThreadAuthor()
    {
        var thread = new UserThread(Guid.NewGuid(), null, null, "Review text", false);
        SetupThreads([thread]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(thread.Id, "Nice review!", null),
            CancellationToken.None);

        _notificationMock.Verify(
            x => x.SendToUser(thread.UserId, It.IsAny<NotificationMessage>()),
            Times.Once);
    }

    [Test]
    public async Task Should_Send_Notification_To_Both_When_TargetedUserId_Provided()
    {
        var targetedUserId = Guid.NewGuid();
        var targetedUser = new User(targetedUserId, "targeted", "targeted@test.com");

        var thread = new UserThread(Guid.NewGuid(), null, null, "Review text", false);
        SetupThreads([thread], [targetedUser]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(thread.Id, "Replying to your comment!", targetedUserId),
            CancellationToken.None);

        _notificationMock.Verify(
            x => x.SendToUsers(thread.UserId, targetedUserId, It.IsAny<NotificationMessage>()),
            Times.Once);
    }

    [Test]
    public async Task Should_Work_Without_TargetedUserId()
    {
        var thread = new UserThread(Guid.NewGuid(), null, null, "Review text", false);
        SetupThreads([thread]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(thread.Id, "Just a comment", null),
            CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(201);
        ep.Response.Text.Should().Be("Just a comment");
    }

    [Test]
    public async Task Should_Use_UserId_From_CurrentUserService()
    {
        var expectedUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(expectedUserId);

        var thread = new UserThread(Guid.NewGuid(), null, null, "Review text", false);
        SetupThreads([thread]);

        var dataAccess = new DataAccess(_dbContextMock.Object, _notificationMock.Object, _currentUserMock.Object, _localizerMock.Object);
        var ep = CreateEndpoint(dataAccess);

        await ep.HandleAsync(
            new Command(thread.Id, "Comment text", null),
            CancellationToken.None);

        _notificationMock.Verify(
            x => x.SendToUser(
                It.IsAny<Guid>(),
                It.Is<NotificationMessage>(m => m.SenderId == expectedUserId.ToString())),
            Times.Once);
    }
}