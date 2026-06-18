using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.SaveFilmsList;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class SaveFilmsListTests
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
        return Factory.Create<Endpoint>(_dataAccess, _currentUserMock.Object);
    }

    private void SetupData(List<FilmsList> lists, List<SavedList> savedLists)
    {
        _dbContextMock.Setup(x => x.FilmsLists).Returns(lists.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SavedLists).Returns(savedLists.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task SaveFilmsList_Should_Return_404_When_List_Not_Found()
    {
        // Arrange
        SetupData([], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(Guid.NewGuid()), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task SaveFilmsList_Should_Return_403_When_List_Is_Private()
    {
        // Arrange
        var list = Helpers.CreateFilmsList(isPrivate: true);
        SetupData([list], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
    }

    [Test]
    public async Task SaveFilmsList_Should_Return_204_And_Add_When_Not_Yet_Saved()
    {
        // Arrange
        var list = Helpers.CreateFilmsList(isPrivate: false);
        SetupData([list], []);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SavedLists.Add(It.IsAny<SavedList>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SaveFilmsList_Should_Return_204_And_Not_Add_When_Already_Saved()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(isPrivate: false);
        var savedList = new SavedList(userId, list.Id);
        SetupData([list], [savedList]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SavedLists.Add(It.IsAny<SavedList>()), Times.Never);
    }
}
