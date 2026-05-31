using FastEndpoints;
using FluentAssertions;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Reviews.GetReviews;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class GetReviewsTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private DataAccess _dataAccess = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.LangCulture).Returns("en");

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(new List<Review>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null) =>
        Factory.Create<Endpoint>(
            dataAccess ?? _dataAccess,
            _currentUserMock.Object);

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
    public async Task GetReviews_Should_Return_200_When_Empty()
    {
        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().BeEmpty();
    }

    [Test]
    public async Task GetReviews_Should_Return_200_With_Data()
    {
        var reviews = Helpers.CreateReviews(3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().NotBeEmpty();
    }

    // ──────────────────────────────────────────────
    // Маппинг
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetReviews_Should_Map_Response_Correctly()
    {
        var reviews = Helpers.CreateReviews(1);
        SetupReviews(reviews);

        var review = reviews.First();
        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        var item = ep.Response.Results.First();

        item.Id.Should().Be(review.Id);
        item.UserId.Should().Be(review.UserId);
        item.Username.Should().Be(review.Reviewer.Username);
        item.AvatarUrl.Should().Be(review.Reviewer.AvatarUrl);
        item.Title.Should().Be(review.Title);
        item.Text.Should().Be(review.Text);
        item.LikesCount.Should().Be(review.LikesCount);
        item.IsSpoilerFree.Should().Be(review.IsSpoilerFree);
        item.FilmId.Should().Be(review.Film.ExternalId);
        item.FilmSlug.Should().Be(review.Film.Slug);
        item.FilmPosterPath.Should().Be(review.Film.PosterPath);
        item.CreatedAt.Should().BeCloseTo(review.CreatedAt, TimeSpan.FromSeconds(5));
    }

    // ──────────────────────────────────────────────
    // Пагинация и TotalCount
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetReviews_Should_Apply_Pagination()
    {
        var reviews = Helpers.CreateReviews(10);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Page = 2, PageSize = 3 }, CancellationToken.None);

        ep.Response.Results.Count.Should().Be(3);
    }

    [Test]
    public async Task GetReviews_Should_Return_Correct_TotalCount()
    {
        var reviews = Helpers.CreateReviews(7);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Page = 1, PageSize = 5 }, CancellationToken.None);

        ep.Response.TotalResults.Should().Be(7);
    }

    // ──────────────────────────────────────────────
    // Сортировка
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetReviews_Should_Sort_By_MostRecent()
    {
        var reviews = Helpers.CreateReviews(3);
        reviews[0].SetCreatedAt(DateTime.UtcNow.AddDays(-2));
        reviews[1].SetCreatedAt(DateTime.UtcNow.AddDays(-1));
        reviews[2].SetCreatedAt(DateTime.UtcNow);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { SortBy = ContentOrderEnum.MostRecent }, CancellationToken.None);

        ep.Response.Results.Should().BeInDescendingOrder(x => x.CreatedAt);
    }

    [Test]
    public async Task GetReviews_Should_Sort_By_MostLiked()
    {
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { SortBy = ContentOrderEnum.MostLiked }, CancellationToken.None);

        ep.Response.Results.Should().BeInDescendingOrder(x => x.LikesCount);
    }

    [Test]
    public async Task GetReviews_Should_Sort_By_HighestRated()
    {
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { SortBy = ContentOrderEnum.HighestRated }, CancellationToken.None);

        ep.Response.Results.Should().BeInDescendingOrder(x => x.Rating);
    }

    [Test]
    public async Task GetReviews_Should_Sort_By_MostReplies()
    {
        var reviews = Helpers.CreateReviewsWithComments(5, commentsPerReview: 3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { SortBy = ContentOrderEnum.MostReplies }, CancellationToken.None);

        ep.Response.Results.Should().BeInDescendingOrder(x => x.RepliesCount);
    }

    // ──────────────────────────────────────────────
    // Фильтрация по рейтингу
    // ──────────────────────────────────────────────

    [Test]
    public async Task GetReviews_Should_Filter_MoreThanFourHalf()
    {
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Filter = RatingEnum.MoreThanFourHalf }, CancellationToken.None);

        ep.Response.Results.Should().OnlyContain(x => x.Rating >= 4.5f && x.Rating <= 5f);
    }

    [Test]
    public async Task GetReviews_Should_Filter_FourStars()
    {
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Filter = RatingEnum.FourStars }, CancellationToken.None);

        ep.Response.Results.Should().OnlyContain(x => x.Rating >= 4f && x.Rating < 4.5f);
    }

    [Test]
    public async Task GetReviews_Should_Filter_ThreeStars()
    {
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Filter = RatingEnum.ThreeStars }, CancellationToken.None);

        ep.Response.Results.Should().OnlyContain(x => x.Rating >= 3f && x.Rating < 4f);
    }

    [Test]
    public async Task GetReviews_Should_Filter_UnderThree()
    {
        var reviews = Helpers.CreateReviews();
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Filter = RatingEnum.UnderThree }, CancellationToken.None);

        ep.Response.Results.Should().OnlyContain(x => x.Rating < 3f);
    }

    [Test]
    public async Task Should_Filter_LongForm()
    {
        var longReviews = Helpers.CreateReviews(3, 600);
        var shortReviews = Helpers.CreateReviews(3, 100);

        foreach (var review in longReviews)
            review.SetText(new string('a', 600));

        foreach (var review in shortReviews)
            review.SetText(new string('a', 100));
        
        SetupReviews([..longReviews, ..shortReviews]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Category = ContentFilterEnum.LongForm }, CancellationToken.None);

        ep.Response.Results.Should().OnlyContain(x => x.Text.Length >= 500);
    }


    [Test]
    public async Task Should_Set_IsLikedByMe_False_For_Anonymous()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.Empty);

        var reviews = Helpers.CreateReviews(3);
        SetupReviews(reviews);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        ep.Response.Results.Should().OnlyContain(x => !x.IsLikedByMe);
    }
}