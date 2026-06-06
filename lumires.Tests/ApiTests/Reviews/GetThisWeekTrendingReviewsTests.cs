using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Reviews.GetThisWeekTrendingReviews;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class GetThisWeekTrendingReviewsTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.LangCulture).Returns("en");

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(new List<Review>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            dataAccess ?? _dataAccess);
    }

    private void SetupReviews(List<Review> reviews)
    {
        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(reviews.BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    // ──────────────────────────────────────────────
    // Статус-коды
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetTrendingReviews_Should_Return_200_When_Empty()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Items.Should().BeEmpty();
    }

    [Test]
    public async Task GetTrendingReviews_Should_Return_200_With_Data()
    {
        // Arrange
        var reviews = Helpers.CreateTrendingReviews(3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Items.Should().NotBeEmpty();
    }


    [Test]
    public async Task GetTrendingReviews_Should_Exclude_Reviews_Older_Than_7_Days()
    {
        // Arrange
        var oldReviews = Helpers.CreateTrendingReviews(3, 14);
        SetupReviews(oldReviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.Items.Should().BeEmpty();
    }

    [Test]
    public async Task GetTrendingReviews_Should_Only_Include_Reviews_Within_7_Days()
    {
        // Arrange
        var recent = Helpers.CreateTrendingReviews(3, 3);
        var old = Helpers.CreateTrendingReviews(3, 14);
        SetupReviews([..recent, ..old]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.Items.Count.Should().Be(3);
    }

    // ──────────────────────────────────────────────
    // Фильтрация: только с непустым Title
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetTrendingReviews_Should_Exclude_Reviews_Without_Title()
    {
        // Arrange
        var withTitle = Helpers.CreateTrendingReviews(3, withTitle: true);
        var noTitle = Helpers.CreateTrendingReviews(3, withTitle: false);
        SetupReviews([..withTitle, ..noTitle]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.Items.Count.Should().Be(3);
    }

    [Test]
    public async Task GetTrendingReviews_Should_Return_At_Most_Six_Items()
    {
        // Arrange
        var reviews = Helpers.CreateTrendingReviews(20);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        ep.Response.Items.Count.Should().BeLessThanOrEqualTo(6);
    }

    [Test]
    public async Task GetTrendingReviews_Should_Sort_By_Engagement_Score_Descending()
    {
        var reviews = Helpers.CreateTrendingReviewsWithEngagement(
        [
            (likesCount: 10, commentsCount: 0),
            (likesCount: 0, commentsCount: 10),
            (likesCount: 5, commentsCount: 3)
        ]);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        // Assert 
        ep.Response.Items[0].Username.Should().Be("user_score20");
        ep.Response.Items[1].Username.Should().Be("user_score11");
        ep.Response.Items[2].Username.Should().Be("user_score10");
    }

    [Test]
    public async Task GetTrendingReviews_Should_Map_Response_Correctly()
    {
        // Arrange
        var reviews = Helpers.CreateTrendingReviews(1);
        SetupReviews(reviews);

        var review = reviews.First();
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);

        var item = ep.Response.Items.First();

        // Assert
        item.FilmId.Should().Be(review.Film.ExternalId);
        item.ReviewTitle.Should().Be(review.Title);
        item.Rating.Should().Be(review.Rating);
        item.UserId.Should().Be(review.UserId);
        item.Username.Should().Be(review.Reviewer.Username);
    }

    // ──────────────────────────────────────────────
    // LangCulture
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetTrendingReviews_Should_ReadLangCulture_OnEveryRequest()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(CancellationToken.None);
        await ep.HandleAsync(CancellationToken.None);
        await ep.HandleAsync(CancellationToken.None);

        // Assert
        _currentUserMock.Verify(x => x.LangCulture, Times.Exactly(3));
    }
}