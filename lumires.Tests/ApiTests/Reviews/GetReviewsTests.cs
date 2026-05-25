using FastEndpoints;
using FluentAssertions;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Reviews.GetReviewsByFilm;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Films;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class GetReviewsTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<IFilmResolver> _resolverMock = null!;


    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _resolverMock = new Mock<IFilmResolver>();
        _resolverMock
            .Setup(x => x.EnsureFilmExistsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);


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
            _currentUserMock.Object,
            _resolverMock.Object);
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
            FilmId = reviews.First().Film.ExternalId,
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
            FilmId = 1
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
            FilmId = reviews.First().Film.ExternalId,
            Filter = RatingEnum.MoreThanFourHalf
        }, CancellationToken.None);

        // Assert
        ep.Response.Results.Should().OnlyContain(x => x.Rating >= 4.5f && x.Rating <= 5f);
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
            FilmId = reviews.First().Film.ExternalId,
            SortBy = ContentOrderEnum.MostLiked
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
            FilmId = reviews.First().Film.ExternalId,
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
            FilmId = reviews.First().Film.ExternalId,
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        // Assert
        ep.Response.TotalResults.Should().Be(7);
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
            FilmId = review.Film.ExternalId
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