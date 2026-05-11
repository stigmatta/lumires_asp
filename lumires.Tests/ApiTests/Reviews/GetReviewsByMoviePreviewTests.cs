using FastEndpoints;
using FluentAssertions;
using JetBrains.Annotations;
using lumires.Api.Features.Reviews.GetReviewsByMoviePreview;
using lumires.Api.Services;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class GetReviewsPreviewTests
{
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<IMovieResolver> _resolverMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        _resolverMock = new Mock<IMovieResolver>();
        _resolverMock
            .Setup(x => x.EnsureMovieExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _dbContextMock
            .Setup(x => x.Movies)
            .Returns(new List<Movie>().BuildMockDbSet().Object);

        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(new List<Review>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        var da = dataAccess ?? _dataAccess;

        return Factory.Create<Endpoint>(
            ctx =>
            {
                var services = new ServiceCollection();
                services.AddSingleton(da);
                services.AddSingleton(Mock.Of<LinkGenerator>());
                services.AddRouting();
                ctx.RequestServices = services.BuildServiceProvider();
            },
            da,
            _resolverMock.Object);
    }

    // ============================
    // TESTS
    // ============================

    [Test]
    public async Task Should_Return_200_With_Data()
    {
        // Arrange
        var reviews = Helpers.CreateReviewsWithComments(3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(reviews.First().Movie.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Reviews.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_Return_Empty_When_No_Reviews()
    {
        // Arrange
        SetupReviews([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Reviews.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Return_NotFound_When_Movie_Not_Exists()
    {
        // Arrange
        _dbContextMock
            .Setup(x => x.Movies)
            .Returns(new List<Movie>().BuildMockDbSet().Object);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task Should_Return_Empty_When_Movie_Just_Imported()
    {
        // Arrange
        var reviews = Helpers.CreateReviewsWithComments(3);
        SetupReviews(reviews);

        _resolverMock
            .Setup(x => x.EnsureMovieExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); 

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(reviews.First().Movie.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Reviews.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Order_By_Likes_Descending()
    {
        // Arrange
        var reviews = Helpers.CreateReviewsWithComments(5);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(reviews.First().Movie.ExternalId), CancellationToken.None);

        // Assert
        ep.Response.Reviews.Should().BeInDescendingOrder(x => x.LikeCount);
    }

    [Test]
    public async Task Should_Map_Review_Correctly()
    {
        // Arrange
        var reviews = Helpers.CreateReviewsWithComments(1);
        SetupReviews(reviews);

        var review = reviews.First();

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(review.Movie.ExternalId), CancellationToken.None);

        var item = ep.Response.Reviews.First();

        // Assert
        item.UserId.Should().Be(review.UserId);
        item.Username.Should().Be(review.Reviewer.Username);
        item.AvatarUrl.Should().Be(review.Reviewer.AvatarUrl);
        item.Text.Should().Be(review.Text);
        item.LikeCount.Should().Be(review.LikesCount);
        item.ReplyCount.Should().Be(review.ReviewComments.Count);
    }

    [Test]
    public async Task Should_Map_Top_Comment()
    {
        // Arrange
        var reviews = Helpers.CreateReviewsWithComments(1);
        SetupReviews(reviews);

        var review = reviews.First();
        var expectedComment = review.ReviewComments
            .OrderByDescending(x => x.LikesCount)
            .First();

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(review.Movie.ExternalId), CancellationToken.None);

        var item = ep.Response.Reviews.First();

        // Assert
        item.Comment.Should().NotBeNull();
        item.Comment!.UserId.Should().Be(expectedComment.UserId);
        item.Comment.Username.Should().Be(expectedComment.Commentator.Username);
        item.Comment.AvatarUrl.Should().Be(expectedComment.Commentator.AvatarUrl);
        item.Comment.Text.Should().Be(expectedComment.Text);
    }


    private void SetupReviews(List<Review> reviews)
    {
        var reviewsMock = reviews.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Reviews).Returns(reviewsMock.Object);

        var movieId = reviews.FirstOrDefault()?.Movie?.ExternalId ?? 1;

        _dbContextMock.Setup(x => x.Movies)
            .Returns(new List<Movie>
            {
                new(movieId, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 8.0f, 100, 50f)
            }.BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }
}