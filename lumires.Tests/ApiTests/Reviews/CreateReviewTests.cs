using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Reviews.CreateReview;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class CreateReviewTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _dbContextMock = new Mock<IAppDbContext>();

        var movies = new List<Film>
        {
            new(1, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 4.0f, 100, 50f, 200, "HBO")
        }.BuildMockDbSet();

        _dbContextMock
            .Setup(x => x.Films)
            .Returns(movies.Object);

        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(new List<Review>().BuildMockDbSet().Object);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        var da = dataAccess ?? _dataAccess;
        return Factory.Create<Endpoint>(
            ctx =>
            {
                var services = new ServiceCollection();
                services.AddSingleton(_currentUserMock.Object);
                services.AddSingleton(da);
                services.AddSingleton(Mock.Of<LinkGenerator>());
                services.AddRouting();
                ctx.RequestServices = services.BuildServiceProvider();
            },
            _currentUserMock.Object,
            da);
    }

    [Test]
    public async Task CreateReview_Should_Be_201_When_Successfully_Created()
    {
        // Arrange
        var movie = SetupMovieExists();

        var ep = CreateEndpoint(_dataAccess);

        // Act
        await ep.HandleAsync(
            new Command(movie.ExternalId, "Great film", "Really enjoyed it", 4.5m, true),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task CreateReview_Should_Return_Correct_Response()
    {
        // Arrange
        var movie = SetupMovieExists();

        var ep = CreateEndpoint(_dataAccess);
        var command = new Command(movie.ExternalId, "Great film", "Really enjoyed it", 4.5m, true);

        // Act
        await ep.HandleAsync(command, CancellationToken.None);

        // Assert
        ep.Response.Title.Should().Be(command.Title);
        ep.Response.Text.Should().Be(command.Text);
        ep.Response.Rating.Should().Be(command.Rating);
        ep.Response.Id.Should().NotBe(Guid.Empty);
        ep.Response.CreatedAt.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Test]
    public async Task CreateReview_Should_Be_404_When_Movie_NotFound()
    {
        // Arrange
        _dbContextMock
            .Setup(x => x.Films)
            .Returns(new List<Film>().BuildMockDbSet().Object);

        var dataAccess = new DataAccess(_dbContextMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(
            new Command(1, "Title", "Text", 4.0m, false),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task CreateReview_Should_Use_UserId_From_CurrentUserService()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(expectedUserId);

        Review? savedReview = null;
        var reviewsDbSetMock = new Mock<DbSet<Review>>();
        reviewsDbSetMock
            .Setup(x => x.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
            .Callback<Review, CancellationToken>((r, _) => savedReview = r)
            .ReturnsAsync((EntityEntry<Review>)null!);

        var movie = SetupMovieExists();
        _dbContextMock.Setup(x => x.Reviews).Returns(reviewsDbSetMock.Object);

        var dataAccess = new DataAccess(_dbContextMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(
            new Command(movie.ExternalId, "Title", "Text", 4.0m, false),
            CancellationToken.None);

        // Assert
        savedReview.Should().NotBeNull();
        savedReview!.UserId.Should().Be(expectedUserId);
    }

    [Test]
    public async Task CreateReview_Should_Work_Without_Title()
    {
        // Arrange
        var movie = SetupMovieExists();

        var ep = CreateEndpoint(_dataAccess);

        // Act
        await ep.HandleAsync(
            new Command(movie.ExternalId, null, "Just some thoughts", 3.0m, false),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(201);
        ep.Response.Title.Should().BeNull();
    }

    [Test]
    public async Task CreateReview_Should_Work_Without_Rating()
    {
        // Arrange
        var movie = SetupMovieExists();

        var ep = CreateEndpoint(_dataAccess);

        // Act
        await ep.HandleAsync(
            new Command(movie.ExternalId, "Title", "Text", null, true),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(201);
        ep.Response.Rating.Should().BeNull();
    }

    private Film SetupMovieExists()
    {
        var movie = new Film(1, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 4.0f, 100, 50f, 200, "HBO");
        var movies = new List<Film> { movie }.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Films).Returns(movies.Object);

        return movie;
    }
}