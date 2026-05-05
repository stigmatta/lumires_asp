using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Collections.Commands.CreateCollection;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using lumires.Domain.Exceptions;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class CreateCollectionTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());
        _currentUserMock
            .Setup(x => x.LangCulture)
            .Returns("en-US");

        var dbContextMock = new Mock<IAppDbContext>();

        dbContextMock
            .Setup(x => x.Collections)
            .Returns(new List<Collection>().BuildMockDbSet().Object);

        dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dataAccess = new DataAccess(dbContextMock.Object);
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
        await act.Should().ThrowAsync<CollectionValidationException>();
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
        ep.Response.CollectionId.Should().NotBe(Guid.Empty);
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

        Collection? savedCollection = null;
        var dbContextMock = new Mock<IAppDbContext>();
        var collectionsDbSetMock = new Mock<DbSet<Collection>>();
        collectionsDbSetMock
            .Setup(x => x.Add(It.IsAny<Collection>()))
            .Callback<Collection>(c => savedCollection = c);
        dbContextMock.Setup(x => x.Collections).Returns(collectionsDbSetMock.Object);
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dataAccess = new DataAccess(dbContextMock.Object);
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
        var movieIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        Collection? savedCollection = null;
        var dbContextMock = new Mock<IAppDbContext>();
        var collectionsDbSetMock = new Mock<DbSet<Collection>>();
        collectionsDbSetMock
            .Setup(x => x.Add(It.IsAny<Collection>()))
            .Callback<Collection>(c => savedCollection = c);
        dbContextMock.Setup(x => x.Collections).Returns(collectionsDbSetMock.Object);
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dataAccess = new DataAccess(dbContextMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(
            new Command("Valid Title", "Description", false, movieIds),
            CancellationToken.None);

        // Assert
        savedCollection.Should().NotBeNull();
        savedCollection!.Movies.Should().HaveCount(2);
        savedCollection.Movies.Select(m => m.MovieId).Should().BeEquivalentTo(movieIds);
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