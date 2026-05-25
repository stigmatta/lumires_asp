using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Reviews.CreateReviewComment;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class CreateReviewCommentTests
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

        _dbContextMock
            .Setup(x => x.ReviewComments)
            .Returns(new List<ReviewComment>().BuildMockDbSet().Object);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationMock
            .Setup(x => x.SendToUsersAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<NotificationMessage>()))
            .Returns(Task.CompletedTask);

        _dataAccess = new DataAccess(_dbContextMock.Object, _notificationMock.Object, _currentUserMock.Object);
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

    private void SetupReviews(List<Review> reviews)
    {
        var mock = reviews.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Reviews).Returns(mock.Object);
        _dataAccess = new DataAccess(_dbContextMock.Object, _notificationMock.Object, _currentUserMock.Object);
    }

    [Test]
    public async Task CreateReviewComment_Should_Be_201_When_Successfully_Created()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), null, "Review text", 4.0f, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(review.Id, "Great review!", null),
            CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task CreateReviewComment_Should_Return_Correct_Response()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), null, "Review text", 4.0f, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();
        var command = new Command(review.Id, "Great review!", null);

        await ep.HandleAsync(command, CancellationToken.None);

        ep.Response.Text.Should().Be(command.Text);
        ep.Response.Id.Should().NotBe(Guid.Empty);
        ep.Response.CreatedAt.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Test]
    public async Task CreateReviewComment_Should_Be_404_When_Review_NotFound()
    {
        SetupReviews([]);
        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(Guid.NewGuid(), "Some comment", null),
            CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task CreateReviewComment_Should_Send_Notification_To_ReviewAuthor()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), null, "Review text", 4.0f, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(review.Id, "Nice review!", null),
            CancellationToken.None);

        _notificationMock.Verify(
            x => x.SendToUsersAsync(review.UserId, null, It.IsAny<NotificationMessage>()),
            Times.Once);
    }

    [Test]
    public async Task CreateReviewComment_Should_Send_Notification_To_Both_When_TargetedUserId_Provided()
    {
        var targetedUserId = Guid.NewGuid();
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), null, "Review text", 4.0f, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(review.Id, "Replying to your comment!", targetedUserId),
            CancellationToken.None);

        _notificationMock.Verify(
            x => x.SendToUsersAsync(review.UserId, targetedUserId, It.IsAny<NotificationMessage>()),
            Times.Once);
    }

    [Test]
    public async Task CreateReviewComment_Should_Work_Without_TargetedUserId()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), null, "Review text", 4.0f, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(
            new Command(review.Id, "Just a comment", null),
            CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(201);
        ep.Response.Text.Should().Be("Just a comment");
    }

    [Test]
    public async Task CreateReviewComment_Should_Use_UserId_From_CurrentUserService()
    {
        var expectedUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(expectedUserId);

        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), null, "Review text", 4.0f, false);
        SetupReviews([review]);

        var dataAccess = new DataAccess(_dbContextMock.Object, _notificationMock.Object, _currentUserMock.Object);
        var ep = CreateEndpoint(dataAccess);

        await ep.HandleAsync(
            new Command(review.Id, "Comment text", null),
            CancellationToken.None);

        _notificationMock.Verify(
            x => x.SendToUsersAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.Is<NotificationMessage>(m => m.SenderId == expectedUserId.ToString())),
            Times.Once);
    }
}