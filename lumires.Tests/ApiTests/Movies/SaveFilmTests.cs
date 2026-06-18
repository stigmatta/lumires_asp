using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Films.SaveFilm;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Movies;

internal sealed class SaveFilmTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<IFilmResolver> _filmResolverMock = null!;
    private DataAccess _dataAccess = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.LangCulture).Returns("en-US");

        _filmResolverMock = new Mock<IFilmResolver>();
        _filmResolverMock
            .Setup(x => x.EnsureFilmExistsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        return Factory.Create<Endpoint>(
            _currentUserMock.Object,
            dataAccess ?? _dataAccess,
            _filmResolverMock.Object);
    }

    private void SetupData(List<Film> films, List<SavedFilm> savedFilms)
    {
        _dbContextMock.Setup(x => x.Films).Returns(films.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SavedFilms).Returns(savedFilms.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task SaveFilm_Should_Return_404_When_Film_Not_Found()
    {
        // Arrange
        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task SaveFilm_Should_Return_204_And_Add_When_Not_Yet_Saved()
    {
        // Arrange
        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        SetupData([film], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SavedFilms.Add(It.IsAny<SavedFilm>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SaveFilm_Should_Return_204_And_Not_Add_When_Already_Saved()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        var saved = new SavedFilm(userId, film.Id);
        typeof(SavedFilm).GetProperty("Film")!.SetValue(saved, film);
        SetupData([film], [saved]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SavedFilms.Add(It.IsAny<SavedFilm>()), Times.Never);
    }

    [Test]
    public async Task SaveFilm_Should_Store_Correct_UserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(expectedUserId);

        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        SavedFilm? saved = null;

        _dbContextMock.Setup(x => x.Films).Returns(new List<Film> { film }.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SavedFilms).Returns(new List<SavedFilm>().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SavedFilms.Add(It.IsAny<SavedFilm>()))
            .Callback<SavedFilm>(s => saved = s);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        var dataAccess = new DataAccess(_dbContextMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(new Command(film.ExternalId), CancellationToken.None);

        // Assert
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(expectedUserId);
    }
}
