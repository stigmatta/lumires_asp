using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.AddFilmToFilmList;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class AddFilmToFilmsListTests
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

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess, _filmResolverMock.Object);
    }

    private void SetupData(List<FilmsList> lists, List<Film> films)
    {
        _dbContextMock.Setup(x => x.FilmsLists).Returns(lists.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Films).Returns(films.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task AddFilmToFilmsList_Should_Return_404_When_List_Not_Found()
    {
        // Arrange
        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(Guid.NewGuid(), 1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task AddFilmToFilmsList_Should_Return_404_When_List_Belongs_To_Other_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(userId: otherUserId);
        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        SetupData([list], [film]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id, film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task AddFilmToFilmsList_Should_Return_404_When_Film_Not_Found_In_Db()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(userId: userId);
        SetupData([list], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id, 999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task AddFilmToFilmsList_Should_Return_204_And_SaveChanges_When_Film_Added()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(userId: userId);
        var film = Helpers.CreateFilmsWithVoteAverage([4.0f]).First();
        SetupData([list], [film]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id, film.ExternalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
