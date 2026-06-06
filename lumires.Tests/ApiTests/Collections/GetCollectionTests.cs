using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.GetFilmsList;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class GetCollectionTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;


    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();

        _currentUserMock
            .Setup(x => x.LangCulture)
            .Returns("en");

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(new List<FilmsList>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(dbContextMock.Object);
    }

    [Test]
    public async Task GetCollection_Should_Be_200_When_Collection_Exists()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var user = new User(userId, "test_user", "test_user@gmail.com");

        var collection = new FilmsList("Test Title", user.Id, "Test description");

        typeof(FilmsList)
            .GetProperty(nameof(FilmsList.User))!
            .SetValue(collection, user);

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(new List<FilmsList> { collection }.BuildMockDbSet().Object);

        var dataAccess = new DataAccess(dbContextMock.Object);

        var ep = Factory.Create<Endpoint>(
            _currentUserMock.Object,
            dataAccess);

        // Act
        await ep.HandleAsync(new Query(collection.Id), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Id.Should().Be(collection.Id);
        ep.Response.Title.Should().Be("Test Title");
    }

    [Test]
    public async Task GetCollection_Should_Be_404_When_Collection_NotExists()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var user = new User(userId, "test_user", "test_user@gmail.com");

        var collection = new FilmsList("Test Title", user.Id, "Test description");

        typeof(FilmsList)
            .GetProperty(nameof(FilmsList.User))!
            .SetValue(collection, user);

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(new List<FilmsList> { collection }.BuildMockDbSet().Object);

        var dataAccess = new DataAccess(dbContextMock.Object);

        var ep = Factory.Create<Endpoint>(
            _currentUserMock.Object,
            dataAccess);

        var randGuid = Guid.CreateVersion7();

        // Act
        await ep.HandleAsync(new Query(randGuid), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public async Task GetCollection_Should_Be_404_When_CollectionId_Empty()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var user = new User(userId, "test_user", "test_user@gmail.com");

        var collection = new FilmsList("Test Title", user.Id, "Test description");

        typeof(FilmsList)
            .GetProperty(nameof(FilmsList.User))!
            .SetValue(collection, user);

        var dbContextMock = new Mock<IAppDbContext>();
        dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(new List<FilmsList> { collection }.BuildMockDbSet().Object);

        var dataAccess = new DataAccess(dbContextMock.Object);

        var ep = Factory.Create<Endpoint>(
            _currentUserMock.Object,
            dataAccess);

        var emptyGuid = Guid.Empty;

        // Act
        await ep.HandleAsync(new Query(emptyGuid), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }
}