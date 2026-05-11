using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Reviews.LikeReview;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

public class LikeReviewTests
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

    private void SetupReviews(List<Review> reviews)
    {
        var mock = reviews.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Reviews).Returns(mock.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object, _currentUserMock.Object, _notificationMock.Object);
    }

    [Test]
    public async Task LikeReview_Should_Return_404_When_Review_NotFound()
    {
        // Arrange
        SetupReviews([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(Guid.NewGuid()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task LikeReview_Should_Return_200_When_Liked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var review = new Review(userId, Guid.NewGuid(), null, "Text", 4.0m, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(review.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task LikeReview_Should_Return_IsLiked_True_On_First_Like()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var review = new Review(userId, Guid.NewGuid(), null, "Text", 4.0m, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(review.Id), CancellationToken.None);

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

        var review = new Review(userId, Guid.NewGuid(), null, "Text", 4.0m, false);
        review.ToggleLike(userId);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(review.Id), CancellationToken.None);

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

        var review = new Review(userId, Guid.NewGuid(), null, "Text", 4.0m, false);
        SetupReviews([review]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(review.Id), CancellationToken.None);

        // Assert
        ep.Response.LikesCount.Should().Be(1);
    }
}