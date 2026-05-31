using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Threads.CreateThread;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Threads;

internal sealed class CreateThreadTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _dbContextMock = new Mock<IAppDbContext>();

        _dbContextMock
            .Setup(x => x.Threads)
            .Returns(new List<UserThread>().BuildMockDbSet().Object);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dataAccess = new DataAccess(_dbContextMock.Object);
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
    public async Task Should_Be_201_When_Successfully_Created()
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(
            new Command("Great film", "Really enjoyed it",  true),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(201);
    }

    [Test]
    public async Task Should_Return_Correct_Response()
    {
        // Arrange

        var ep = CreateEndpoint();
        var command = new Command("Great film", "Really enjoyed it", true);

        // Act
        await ep.HandleAsync(command, CancellationToken.None);

        // Assert
        ep.Response.Title.Should().Be(command.Title);
        ep.Response.Text.Should().Be(command.Text);
        ep.Response.Id.Should().NotBe(Guid.Empty);
        ep.Response.CreatedAt.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
        ep.Response.IsSpoilerFree.Should().Be(true);
    }

    [Test]
    public async Task Should_Use_UserId_From_CurrentUserService()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(expectedUserId);

        UserThread? savedThread = null;
        var threadsDbSetMock = new Mock<DbSet<UserThread>>();
        threadsDbSetMock
            .Setup(x => x.AddAsync(It.IsAny<UserThread>(), It.IsAny<CancellationToken>()))
            .Callback<UserThread, CancellationToken>((r, _) => savedThread = r)
            .ReturnsAsync((EntityEntry<UserThread>)null!);

        _dbContextMock.Setup(x => x.Threads).Returns(threadsDbSetMock.Object);

        var dataAccess = new DataAccess(_dbContextMock.Object);
        var ep = CreateEndpoint(dataAccess);

        // Act
        await ep.HandleAsync(
            new Command("Title", "Text", false),
            CancellationToken.None);

        // Assert
        savedThread.Should().NotBeNull();
        savedThread!.UserId.Should().Be(expectedUserId);
    }

    [Test]
    public async Task Should_Work_Without_Title()
    {
        // Arrange
        var ep = CreateEndpoint(_dataAccess);

        // Act
        await ep.HandleAsync(
            new Command(null, "Just some thoughts",  false),
            CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(201);
        ep.Response.Title.Should().BeNull();
    }

}