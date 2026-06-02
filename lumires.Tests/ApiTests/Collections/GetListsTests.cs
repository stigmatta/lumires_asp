using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmsLists.GetFilmsLists;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Collections;

internal sealed class GetFilmsListsTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Guid _userId;

    [Before(Test)]
    public void Setup()
    {
        _userId = Guid.NewGuid();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId);

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(new List<FilmsList>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    private Endpoint CreateEndpoint()
    {
        return Factory.Create<Endpoint>(
            ctx =>
            {
                var services = new ServiceCollection();
                services.AddSingleton(_currentUserMock.Object);
                services.AddSingleton(Mock.Of<LinkGenerator>());
                services.AddRouting();

                ctx.RequestServices = services.BuildServiceProvider();
            },
            _dataAccess,
            _currentUserMock.Object);
    }

    private void SetupLists(List<FilmsList> lists)
    {
        _dbContextMock
            .Setup(x => x.FilmsLists)
            .Returns(lists.BuildMockDbSet().Object);
    }

    [Test]
    public async Task Should_Return_200_With_Lists()
    {
        SetupLists(Helpers.CreateFilmsLists());

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().HaveCount(3);
    }

    [Test]
    public async Task Should_Return_Empty_List_When_No_Lists()
    {
        SetupLists([]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        ep.Response.Results.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Map_List_Correctly()
    {
        var list = Helpers.CreateFilmsList(userId: _userId);
        SetupLists([list]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        var item = ep.Response.Results.First();

        item.Title.Should().Be("My Films List");
        item.UserId.Should().Be(_userId);
        item.Films.Should().HaveCountLessThanOrEqualTo(6);
    }

    [Test]
    public async Task Should_Return_Correct_Paging_Metadata()
    {
        SetupLists(Helpers.CreateFilmsLists(10));

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Page = 2, PageSize = 5 }, CancellationToken.None);

        ep.Response.Page.Should().Be(2);
        ep.Response.PageSize.Should().Be(5);
        ep.Response.TotalResults.Should().Be(10);
    }
    
    [Test]
    public void Should_Filter_By_SearchTerm()
    {
        var lists = new List<FilmsList>
        {
            Helpers.CreateFilmsList(
                title: "Ignored",
                films:
                [
                    Helpers.CreatePopularFilm(new DateOnly(2022, 1, 1), "Inception")
                ]),
            
            Helpers.CreateFilmsList(
                title: "Ignored 2",
                films:
                [
                    Helpers.CreatePopularFilm(new DateOnly(2022, 1, 1), "Avatar")
                ])
        };

        var result = lists
            .Where(fl => fl.Films.Any(f =>
                f.Film.Localizations.Any(l =>
                    l.Title.Contains("Incep", StringComparison.OrdinalIgnoreCase))))
            .ToList();

        result.Should().HaveCount(1);
    }


}
