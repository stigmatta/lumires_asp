using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.RemoveFilmFromFilmList;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class RemoveFilmFromFilmsListTests
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

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess, _filmResolverMock.Object);
    }

    private void SetupLists(List<FilmsList> lists)
    {
        _dbContextMock.Setup(x => x.FilmsLists).Returns(lists.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    [Test]
    public async Task RemoveFilmFromFilmsList_Should_Return_404_When_List_Not_Found()
    {
        // Arrange
        SetupLists([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(Guid.NewGuid(), 1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task RemoveFilmFromFilmsList_Should_Return_404_When_List_Belongs_To_Other_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(userId: otherUserId);
        SetupLists([list]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id, 1), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task RemoveFilmFromFilmsList_Should_Return_204_When_Film_Not_In_List()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(userId: userId);
        SetupLists([list]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command(list.Id, 999), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
