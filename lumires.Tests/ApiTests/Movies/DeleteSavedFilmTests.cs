using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.DeleteSavedFilm;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Movies;

internal sealed class DeleteSavedFilmTests
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

    private void SetupSavedFilms(List<SavedFilm> savedFilms)
    {
        _dbContextMock.Setup(x => x.SavedFilms).Returns(savedFilms.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task DeleteSavedFilm_Should_Return_204_When_No_Saved_Record()
    {
        // Arrange
        SetupSavedFilms([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteSavedFilm_Should_Return_204_And_Remove_When_Saved_Record_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        var savedFilm = new SavedFilm(userId, film.Id);
        typeof(SavedFilm).GetProperty("Film")!.SetValue(savedFilm, film);

        SetupSavedFilms([savedFilm]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SavedFilms.Remove(savedFilm), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteSavedFilm_Should_Not_Remove_When_Belongs_To_Another_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        var savedFilm = new SavedFilm(otherUserId, film.Id);
        typeof(SavedFilm).GetProperty("Film")!.SetValue(savedFilm, film);

        SetupSavedFilms([savedFilm]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SavedFilms.Remove(It.IsAny<SavedFilm>()), Times.Never);
    }
}
