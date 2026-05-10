using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Reviews.GetReviewsByMovie;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class GetReviewsTests
{
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        var reviews = Helpers.CreateReviews().BuildMockDbSet();

        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(reviews.Object);

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
            da);
    }

    [Test]
    public async Task GetReviews_Should_Return_200_With_Data()
    {
        // Arrange
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            MovieId = reviews.First().MovieId,
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetReviews_Should_Return_Empty_List()
    {
        // Arrange
        SetupReviews([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            MovieId = Guid.NewGuid()
        }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().BeEmpty();
    }

    [Test]
    public async Task GetReviews_Should_Filter_FiveStars()
    {
        // Arrange
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            MovieId = reviews.First().MovieId,
            Filter = FilterEnum.FiveStars
        }, CancellationToken.None);

        // Assert
        ep.Response.Results.Should().OnlyContain(x => x.Rating == 5m);
    }

    [Test]
    public async Task GetReviews_Should_Sort_By_Likes()
    {
        // Arrange
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            MovieId = reviews.First().MovieId,
            SortBy = SortEnum.MostLiked
        }, CancellationToken.None);

        // Assert
        ep.Response.Results.Should().BeInDescendingOrder(x => x.LikesCount);
    }

    [Test]
    public async Task GetReviews_Should_Apply_Pagination()
    {
        // Arrange
        var reviews = Helpers.CreateReviews(10);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            MovieId = reviews.First().MovieId,
            Page = 2,
            PageSize = 3
        }, CancellationToken.None);

        // Assert
        ep.Response.Results.Count.Should().Be(3);
    }

    [Test]
    public async Task GetReviews_Should_Return_Correct_TotalCount()
    {
        // Arrange
        var reviews = Helpers.CreateReviews(7);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            MovieId = reviews.First().MovieId,
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        // Assert
        ep.Response.TotalCount.Should().Be(7);
    }

    [Test]
    public async Task GetReviews_Should_Map_Response_Correctly()
    {
        // Arrange
        var reviews = Helpers.CreateReviews(1);
        SetupReviews(reviews);

        var review = reviews.First();

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            MovieId = review.MovieId
        }, CancellationToken.None);

        var item = ep.Response.Results.First();

        // Assert
        item.Id.Should().Be(review.Id);
        item.UserId.Should().Be(review.UserId);
        item.Username.Should().Be(review.Reviewer.Username);
        item.AvatarUrl.Should().Be(review.Reviewer.AvatarUrl);
        item.Title.Should().Be(review.Title);
        item.Text.Should().Be(review.Text);
        item.LikesCount.Should().Be(review.LikesCount);
        item.CreatedAt.Should().Be(review.CreatedAt);
    }

    private void SetupReviews(List<Review> reviews)
    {
        var mock = reviews.BuildMockDbSet();

        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(mock.Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }
}