using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.CreateFilmsList;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.Films;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using lumires.Domain.Exceptions;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class CreateCollectionTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IStringLocalizer<SharedResource>> _localizerMock = null!;


    [Before(Test)]
    public void Setup()
    {
        _localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        _localizerMock
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());
        _currentUserMock
            .Setup(x => x.LangCulture)
            .Returns("en-US");

        var dbContextMock = new Mock<IAppDbContext>();

        dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(new List<FilmsList>().BuildMockDbSet().Object);

        dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dataAccess = new DataAccess(dbContextMock.Object, _localizerMock.Object);
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
                services.AddSingleton(Mock.Of<IEventHandler<FilmReferencedEvent>>());
                services.AddRouting();
                ctx.RequestServices = services.BuildServiceProvider();
            },
            _currentUserMock.Object,
            da);
    }

    [Test]
    public async Task CreateCollection_Should_Be_201_When_Successfully_Created()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Command("Best movies ever", "Films you should watch at least once",
            false, []), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task CreateCollection_Should_Throw_Error_When_ValidationError()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        var act = async () => await ep.HandleAsync(
            new Command("", "Films you should watch at least once", false, []),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Test]
    public async Task CreateCollection_Should_Return_Correct_Response()
    {
        // Arrange
        var ep = CreateEndpoint();

        var command = new Command("Best movies ever", "Films you should watch at least once", false, []);

        // Act
        await ep.HandleAsync(command, CancellationToken.None);

        // Assert
        ep.Response.Title.Should().Be(command.Title);
        ep.Response.FilmsListId.Should().NotBe(Guid.Empty);
        ep.Response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task CreateCollection_Should_Use_UserId_From_CurrentUserService()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(expectedUserId);

        FilmsList? savedCollection = null;
        var dbContextMock = new Mock<IAppDbContext>();
        var collectionsDbSetMock = new Mock<DbSet<FilmsList>>();
        collectionsDbSetMock
            .Setup(x => x.Add(It.IsAny<FilmsList>()))
            .Callback<FilmsList>(c => savedCollection = c);
        dbContextMock.Setup(x => x.FilmsLists).Returns(collectionsDbSetMock.Object);
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dataAccess = new DataAccess(dbContextMock.Object, _localizerMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(
            new Command("Valid Title", "Description", false, []),
            CancellationToken.None);

        // Assert
        savedCollection.Should().NotBeNull();
        savedCollection!.UserId.Should().Be(expectedUserId);
    }

    [Test]
    public async Task CreateCollection_Should_Add_MovieIds_To_Collection()
    {
        // Arrange
        var movieIds = new List<int> { 550, 551 };

        FilmsList? savedCollection = null;
        var dbContextMock = new Mock<IAppDbContext>();
        var collectionsDbSetMock = new Mock<DbSet<FilmsList>>();
        collectionsDbSetMock
            .Setup(x => x.Add(It.IsAny<FilmsList>()))
            .Callback<FilmsList>(c => savedCollection = c);
        dbContextMock.Setup(x => x.Films)
            .Returns(new List<Film>
            {
                new(550, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 4.0f, 100, 50f, 150, "warner"),
                new(551, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 4.0f, 100, 50f, 150, "warner-2")
            }.BuildMockDbSet().Object);
        dbContextMock.Setup(x => x.FilmsLists).Returns(collectionsDbSetMock.Object);
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var expectedMovieIds = dbContextMock.Object.Films
            .Where(m => movieIds.Contains(m.ExternalId))
            .Select(m => m.Id)
            .ToList();

        var dataAccess = new DataAccess(dbContextMock.Object, _localizerMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(
            new Command("Valid Title", "Description", false, movieIds),
            CancellationToken.None);

        // Assert
        savedCollection!.Films.Should().HaveCount(2);
        savedCollection.Films.Select(m => m.FilmId).Should().BeEquivalentTo(expectedMovieIds);
    }

    [Test]
    public async Task CreateCollection_Should_Not_Throw_When_MovieIds_IsEmpty()
    {
        // Arrange
        var ep = CreateEndpoint(_dataAccess);

        // Act
        var act = async () => await ep.HandleAsync(
            new Command("Valid Title", "Description", false, []),
            CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}