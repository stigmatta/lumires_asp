using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Reviews.UpdateReview;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Reviews;

internal sealed class UpdateReviewTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private DataAccess _dataAccess = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);
    }

    private void SetupData(List<Film> films, List<Review> reviews)
    {
        _dbContextMock.Setup(x => x.Films).Returns(films.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Reviews).Returns(reviews.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task UpdateReview_Should_Return_404_When_Film_Not_Found()
    {
        // Arrange
        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(999, Guid.NewGuid(), "Title", "Updated text", 4.0f), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task UpdateReview_Should_Return_404_When_Review_Not_Found()
    {
        // Arrange
        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        SetupData([film], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, Guid.NewGuid(), "Title", "Updated text", 4.0f),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task UpdateReview_Should_Return_403_When_Not_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        var review = new Review(otherUserId, film.Id, "Title", "Original text", 3.5f);

        SetupData([film], [review]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, review.Id, "New Title", "Updated text", 4.0f),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateReview_Should_Return_204_And_Update_When_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        var review = new Review(userId, film.Id, "Title", "Original text", 3.5f);

        SetupData([film], [review]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, review.Id, "New Title", "Updated text", 5.0f),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.Reviews.Update(review), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateReview_Should_Apply_New_Text_And_Rating()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        var review = new Review(userId, film.Id, "Original Title", "Original text", 3.0f);

        SetupData([film], [review]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(film.ExternalId, review.Id, "Updated Title", "Updated text", 5.0f),
            CancellationToken.None);

        // Assert
        review.Title.Should().Be("Updated Title");
        review.Text.Should().Be("Updated text");
        review.Rating.Should().Be(5.0f);
    }
}
