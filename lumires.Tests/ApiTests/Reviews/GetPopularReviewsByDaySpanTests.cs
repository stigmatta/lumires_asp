using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Reviews.GetPopularReviewsInDaySpan;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using ZiggyCreatures.Caching.Fusion;

namespace Tests.ApiTests.Reviews;

internal sealed class GetPopularReviewsInDaySpanTests
{
    private FusionCache _cache = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _cache = new FusionCache(new FusionCacheOptions());

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.LangCulture).Returns("en");

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(new List<Review>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object, _currentUserMock.Object);
    }

    [After(Test)]
    public void TearDown()
    {
        _cache.Dispose();
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _cache,
            dataAccess ?? _dataAccess);
    }

    private void SetupReviews(List<Review> reviews)
    {
        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(reviews.BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object, _currentUserMock.Object);
    }


    [Test]
    public async Task GetPopularReviews_Should_Return_200_When_Empty()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Items.Should().BeEmpty();
    }

    [Test]
    public async Task GetPopularReviews_Should_Return_200_With_Data()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(2025, 3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Items.Should().NotBeEmpty();
    }


    [Test]
    public async Task GetPopularReviews_Should_Exclude_Reviews_Outside_DaySpan()
    {
        // Arrange
        var oldReviews = Helpers.CreatePopularReviews(2026, 5, 30);
        SetupReviews(oldReviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);

        // Assert
        ep.Response.Items.Should().BeEmpty();
    }

    [Test]
    public async Task GetPopularReviews_Should_Only_Include_Reviews_Within_DaySpan()
    {
        // Arrange
        var recentReviews = Helpers.CreatePopularReviews(2025, 3, 3);
        var oldReviews = Helpers.CreatePopularReviews(2025, 3, 30);
        SetupReviews([..recentReviews, ..oldReviews]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);

        // Assert
        var cutoff = DateTime.UtcNow.AddDays(-7);
        ep.Response.Items.Should().OnlyContain(x => x.CreatedAt >= cutoff);
    }


    [Test]
    public async Task GetPopularReviews_Should_Only_Include_SpoilerFree_Reviews()
    {
        // Arrange
        var spoilerFree = Helpers.CreatePopularReviews(2025, 3, spoilerFree: true);
        var withSpoilers = Helpers.CreatePopularReviews(2025, 3, spoilerFree: false);
        SetupReviews([..spoilerFree, ..withSpoilers]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        // Assert
        ep.Response.Items.Count.Should().Be(3);
    }


    [Test]
    public async Task GetPopularReviews_Should_Sort_By_LikesCount_Descending()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(2026);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        // Assert
        ep.Response.Items.Should().BeInDescendingOrder(x => x.LikesCount);
    }

    [Test]
    public async Task GetPopularReviews_Should_Return_At_Most_Ten_Items()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(2025, 20);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        // Assert
        ep.Response.Items.Count.Should().BeLessThanOrEqualTo(10);
    }


    [Test]
    public async Task GetPopularReviews_Should_Map_Response_Correctly()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(2025, 1);
        SetupReviews(reviews);

        var review = reviews.First();
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        var item = ep.Response.Items.First();

        // Assert
        item.Id.Should().Be(review.Id);
        item.FilmId.Should().Be(review.Film.ExternalId);
        item.FilmSlug.Should().Be(review.Film.Slug);
        item.PosterPath.Should().Be(review.Film.PosterPath);
        item.Runtime.Should().Be(review.Film.Runtime);
        item.Title.Should().Be(review.Title);
        item.Text.Should().Be(review.Text);
        item.UserId.Should().Be(review.UserId);
        item.Username.Should().Be(review.Reviewer.Username);
        item.CreatedAt.Should().Be(review.CreatedAt);
        item.Rating.Should().Be(review.Rating);
        item.LikesCount.Should().Be(review.LikesCount);
        item.RepliesCount.Should().Be(review.ReviewComments.Count);
        item.IsEditorPick.Should().Be(review.IsEditorPick);
    }

    [Test]
    public async Task GetPopularReviews_Should_Map_ReleaseYear_Correctly()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(2022, 1, 5);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        // Assert
        ep.Response.Items.First().ReleaseYear.Should().Be(2022);
    }

    [Test]
    public async Task GetPopularReviews_Should_Map_ReleaseYear_As_Null_When_No_ReleaseDate()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(null, 1, 5);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        // Assert
        ep.Response.Items.First().ReleaseYear.Should().BeNull();
    }

    [Test]
    public async Task GetPopularReviews_Should_Set_IsLikedByMe_False_For_Anonymous()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.Empty);

        var reviews = Helpers.CreatePopularReviews(2025, 3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        // Assert
        ep.Response.Items.Should().OnlyContain(x => !x.IsLikedByMe);
    }

    [Test]
    public async Task GetPopularReviews_Should_Return_CachedResponse_On_SecondCall()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(2025, 3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);
        var firstResponse = ep.Response;

        await ep.HandleAsync(new Query(7), CancellationToken.None);
        var secondResponse = ep.Response;

        // Assert
        secondResponse.Should().BeEquivalentTo(firstResponse);
    }

    [Test]
    public async Task GetPopularReviews_Should_Cache_Separately_Per_DaySpan()
    {
        // Arrange
        var reviews = Helpers.CreatePopularReviews(2021, 5, 3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);
        var responseFor7 = ep.Response;

        await ep.HandleAsync(new Query(30), CancellationToken.None);
        var responseFor30 = ep.Response;

        // Assert — оба запроса достигли БД (разные ключи кэша)
        _dbContextMock.Verify(x => x.Reviews, Times.Exactly(2));
        responseFor7.Should().BeEquivalentTo(responseFor30);
    }

    [Test]
    public async Task GetPopularReviews_Should_Return_CachedData_WhenDataAccessFails_FailSafe()
    {
        // Arrange
        var callCount = 0;
        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.Reviews)
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    return Helpers.CreatePopularReviews(2025, 1).BuildMockDbSet().Object;

                throw new Exception("DB недоступна");
            });

        var dataAccess = new DataAccess(dbContextMock.Object, _currentUserMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);
        await ep.HandleAsync(new Query(7), CancellationToken.None);

        // Assert
        ep.Response.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task GetPopularReviews_Should_ReturnEmptyResponse_WhenNoReviewsExist()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);

        // Assert
        ep.Response.Should().NotBeNull();
        ep.Response.Items.Should().BeEmpty();
    }

    [Test]
    public async Task GetPopularReviews_Should_ReadLangCulture_OnEveryRequest()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(7), CancellationToken.None);
        await ep.HandleAsync(new Query(14), CancellationToken.None);
        await ep.HandleAsync(new Query(30), CancellationToken.None);

        // Assert
        _currentUserMock.Verify(x => x.LangCulture, Times.Exactly(3));
    }
}