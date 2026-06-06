using FastEndpoints;
using FluentAssertions;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Threads.GetThreads;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Threads;

internal sealed class GetThreadsTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;


    [Before(Test)]
    public void Setup()
    {
        _dbContextMock = new Mock<IAppDbContext>();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        var threads = Helpers.CreateThreads().BuildMockDbSet();

        _dbContextMock
            .Setup(x => x.Threads)
            .Returns(threads.Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null)
    {
        var da = dataAccess ?? _dataAccess;

        return Factory.Create<Endpoint>(
            ctx =>
            {
                var services = new ServiceCollection();
                services.AddSingleton(da);
                services.AddSingleton(Mock.Of<LinkGenerator>());
                services.AddRouting();
                ctx.RequestServices = services.BuildServiceProvider();
            },
            da,
            _currentUserMock.Object);
    }

    [Test]
    public async Task Should_Return_200_With_Data()
    {
        // Arrange
        var threads = Helpers.CreateThreads();
        SetupThreads(threads);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_Return_Empty_List()
    {
        // Arrange
        SetupThreads([]);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Sort_By_Likes()
    {
        // Arrange
        var threads = Helpers.CreateThreads(3);
        threads[0].ToggleLike(Guid.CreateVersion7());

        threads[1].ToggleLike(Guid.CreateVersion7());
        threads[1].ToggleLike(Guid.CreateVersion7());

        threads[2].ToggleLike(Guid.CreateVersion7());
        threads[2].ToggleLike(Guid.CreateVersion7());
        threads[2].ToggleLike(Guid.CreateVersion7());

        SetupThreads(threads);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            SortBy = ContentOrderEnum.MostLiked
        }, CancellationToken.None);

        // Assert
        ep.Response.Results.Should().BeInDescendingOrder(x => x.LikesCount);
    }

    [Test]
    public async Task Should_Apply_Pagination()
    {
        // Arrange
        var threads = Helpers.CreateThreads(10);
        SetupThreads(threads);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            Page = 2,
            PageSize = 3
        }, CancellationToken.None);

        // Assert
        ep.Response.Results.Count.Should().Be(3);
    }

    [Test]
    public async Task Should_Return_Correct_TotalCount()
    {
        // Arrange
        var threads = Helpers.CreateThreads(7);
        SetupThreads(threads);

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query
        {
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        // Assert
        ep.Response.TotalResults.Should().Be(7);
    }

    [Test]
    public async Task Should_Map_Response_Correctly()
    {
        // Arrange
        var threads = Helpers.CreateThreads(1);
        SetupThreads(threads);

        var thread = threads.First();

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(), CancellationToken.None);

        var item = ep.Response.Results.First();

        // Assert
        item.Id.Should().Be(thread.Id);
        item.UserId.Should().Be(thread.UserId);
        item.Username.Should().Be(thread.User.Username);
        item.AvatarUrl.Should().Be(thread.User.AvatarUrl);
        item.Title.Should().Be(thread.Title);
        item.Text.Should().Be(thread.Text);
        item.LikesCount.Should().Be(thread.LikesCount);
        item.IsSpoilerFree.Should().Be(thread.IsSpoilerFree);
        item.CreatedAt.Should().Be(thread.CreatedAt);
    }

    private void SetupThreads(List<UserThread> threads)
    {
        var mock = threads.BuildMockDbSet();
        _dbContextMock.Setup(x => x.Threads).Returns(mock.Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }
}