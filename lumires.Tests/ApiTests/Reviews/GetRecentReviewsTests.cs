using FastEndpoints;
using FluentAssertions;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Reviews.GetRecentReviews;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Films;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class GetRecentReviewsTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;


    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());


        var reviews = Helpers.CreateReviews().BuildMockDbSet();

        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(reviews.Object);

        _dbContextMock
            .Setup(x => x.Films)
            .Returns(new List<Film>().BuildMockDbSet().Object);


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
                services.AddSingleton(typeof(IEventHandler<FilmReferencedEvent>),
                    Mock.Of<IEventHandler<FilmReferencedEvent>>());
                services.AddSingleton(typeof(EventBus<FilmReferencedEvent>),
                    new EventBus<FilmReferencedEvent>([Mock.Of<IEventHandler<FilmReferencedEvent>>()]));
                services.AddRouting();
                ctx.RequestServices = services.BuildServiceProvider();
            },
            da,
            _currentUserMock.Object);
    }

    [Test]
    public async Task Should_Return_200_With_Data()
    {
        // Arrange
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_Return_Empty_List()
    {
        // Arrange
        SetupReviews([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Apply_Pagination()
    {
        // Arrange
        var reviews = Helpers.CreateReviews(10);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            Page = 2,
            PageSize = 3
        }, CancellationToken.None);

        // Assert
        ep.Response.Results.Count.Should().Be(3);
    }

    [Test]
    public async Task Should_Return_Correct_TotalCount()
    {
        // Arrange
        var reviews = Helpers.CreateReviews(7);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        // Assert
        ep.Response.TotalResults.Should().Be(7);
    }

    [Test]
    public async Task Should_Map_Response_Correctly()
    {
        // Arrange
        var reviews = Helpers.CreateReviews(1);
        SetupReviews(reviews);

        var review = reviews.First();

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(), CancellationToken.None);

        var item = ep.Response.Results.First();

        // Assert
        item.Id.Should().Be(review.Id);
        item.UserId.Should().Be(review.UserId);
        item.Username.Should().Be(review.Reviewer.Username);
        item.AvatarUrl.Should().Be(review.Reviewer.AvatarUrl);
        item.Title.Should().Be(review.Title);
        item.Text.Should().Be(review.Text);
        item.LikesCount.Should().Be(review.LikesCount);
        item.IsSpoilerFree.Should().Be(review.IsSpoilerFree);
        item.CreatedAt.Should().Be(review.CreatedAt);
    }
    
    [Test]
    public async Task Should_Sort_By_CreatedAt_Descending()
    {
        // Arrange
        var reviews = Helpers.CreateReviews(3);
        reviews[0].SetCreatedAt(DateTime.UtcNow.AddDays(-2));
        reviews[1].SetCreatedAt(DateTime.UtcNow.AddDays(-1));
        reviews[2].SetCreatedAt(DateTime.UtcNow);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(), CancellationToken.None);

        // Assert
        ep.Response.Results.Should().BeInDescendingOrder(x => x.CreatedAt);
    }

    private void SetupReviews(List<Review> reviews)
    {
        var mock = reviews.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Reviews).Returns(mock.Object);

        var movieId = reviews.FirstOrDefault()?.Film?.ExternalId ?? 1;
        _dbContextMock.Setup(x => x.Films)
            .Returns(new List<Film>
            {
                new(movieId, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 4.0f, 100, 50f, 200, "HBO")
            }.BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }
}