using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.UpdateFilmsList;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using Microsoft.Extensions.Localization;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class UpdateFilmsListTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Mock<IStringLocalizer<SharedResource>> _localizerMock = null!;
    private Mock<IFilmResolver> _filmResolverMock = null!;
    private DataAccess _dataAccess = null!;

    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        _localizerMock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.LangCulture).Returns("en-US");

        _filmResolverMock = new Mock<IFilmResolver>();
        _filmResolverMock
            .Setup(x => x.EnsureFilmsExistAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dataAccess = new DataAccess(_dbContextMock.Object, _localizerMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess, _filmResolverMock.Object);
    }

    private void SetupData(List<FilmsList> lists, List<Film>? films = null)
    {
        _dbContextMock.Setup(x => x.FilmsLists).Returns(lists.BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Films).Returns((films ?? []).BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _dataAccess = new DataAccess(_dbContextMock.Object, _localizerMock.Object);
    }

    [Test]
    public async Task UpdateFilmsList_Should_Return_404_When_List_Not_Found()
    {
        // Arrange
        SetupData([]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(Guid.NewGuid(), "New Title", "New description", false, []),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task UpdateFilmsList_Should_Return_403_When_Not_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(userId: otherUserId);
        SetupData([list]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(list.Id, "New Title", "New description", false, []),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(403);
    }

    [Test]
    public async Task UpdateFilmsList_Should_Return_204_And_Update_When_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(userId: userId);
        SetupData([list]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(list.Id, "Updated Title", "Updated description", true, []),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(204);
        _dbContextMock.Verify(x => x.FilmsLists.Update(list), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateFilmsList_Should_Apply_New_Title_And_Description()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var list = Helpers.CreateFilmsList(title: "Original Title", userId: userId, description: "Original desc");
        SetupData([list]);
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command(list.Id, "New Title", "New description", false, []),
            CancellationToken.None);

        // Assert
        list.Title.Should().Be("New Title");
        list.Description.Should().Be("New description");
    }
}
