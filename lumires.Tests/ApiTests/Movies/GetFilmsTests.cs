using FastEndpoints;
using FluentAssertions;
using lumires.Api.Enums.Common;
using lumires.Api.Features.Films.GetFilms;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Movies;

internal sealed class GetFilmsTests
{
    private const string Lang = "en-US";
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.LangCulture).Returns(Lang);


        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.Reviews)
            .Returns(new List<Review>().BuildMockDbSet().Object);

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

    private void SetupFilms(List<Film> films)
    {
        _dbContextMock
            .Setup(x => x.Films)
            .Returns(films.BuildMockDbSet().Object);
    }


    [Test]
    public async Task Should_Return_200_With_Films()
    {
        SetupFilms(Helpers.CreateFilmsWithPopularity([4.5f, 4f, 3f]));

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Results.Should().HaveCount(3);
    }

    [Test]
    public async Task Should_Return_Empty_List_When_No_Films()
    {
        SetupFilms([]);

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        ep.Response.Results.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Map_Film_Correctly()
    {
        SetupFilms(Helpers.CreateFilmsWithPopularity([4.5f]));


        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query(), CancellationToken.None);

        var item = ep.Response.Results.First();

        item.Title.Should().Be("Film 0");
        item.ReleaseYear.Should().Be(2022);
        item.VoteAverage.Should().Be(4.0f);
    }

    [Test]
    public async Task Should_Return_Correct_Paging_Metadata()
    {
        SetupFilms(Helpers.CreateFilmsWithPopularity([4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.5f]));

        var ep = CreateEndpoint();

        await ep.HandleAsync(new Query { Page = 2, PageSize = 5 }, CancellationToken.None);

        ep.Response.Page.Should().Be(2);
        ep.Response.PageSize.Should().Be(5);
        ep.Response.TotalResults.Should().Be(10);
    }


    [Test]
    public void Should_Filter_By_Single_Genre()
    {
        var films = Helpers.CreateFilmsWithGenres(["Action", "Drama"]);

        var filter = Specifications.BuildFilter(
            new Query { Genres = ["Action"] },
            Lang
        ).Compile();

        var result = films.Where(filter).ToList();

        result.Should().HaveCount(1);
        result.Single().Genres
            .Should()
            .Contain(g =>
                g.Localizations.Any(l => l.Name == "Action"));
    }

    [Test]
    public void Should_Filter_By_Multiple_Genres_All_Must_Match()
    {
        var films = Helpers.CreateFilmsWithGenres(["Action", "Comedy"]);

        var filter = Specifications.BuildFilter(
            new Query { Genres = ["Action", "Comedy"] },
            Lang
        ).Compile();

        var result = films.Where(filter).ToList();

        result.Should().HaveCount(1);
    }

    [Test]
    public void Should_Return_Empty_When_No_Genre_Match()
    {
        var films = Helpers.CreateFilmsWithGenres(["Drama"]);

        var filter = Specifications.BuildFilter(
            new Query { Genres = ["Horror"] },
            Lang
        ).Compile();

        var result = films.Where(filter).ToList();

        result.Should().BeEmpty();
    }


    [Test]
    public void Should_Filter_Popular_Films()
    {
        var films = Helpers.CreateFilmsWithPopularity([10f, 100f, 200f]);

        var filter = Specifications.BuildFilter(
            new Query { Content = FilmContentFilter.Popular },
            Lang
        ).Compile();

        var result = films.Where(filter).ToList();

        result.Should().HaveCount(2);
    }


    [Test]
    public void Should_Filter_By_Rating()
    {
        var films = Helpers.CreateFilmsWithVoteAverage([2.0f, 3.0f, 4.6f]);

        var filter = Specifications.BuildFilter(
            new Query { Rating = RatingEnum.MoreThanFourHalf },
            Lang
        ).Compile();

        var result = films.Where(filter).ToList();

        result.Should().HaveCount(1);
        result.Single().VoteAverage.Should().Be(4.6f);
    }


    [Test]
    public void Should_Filter_New_Releases()
    {
        var films = Helpers.CreateFilmsWithReleaseDates([
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-100))
        ]);

        var filter = Specifications.BuildFilter(
            new Query { Content = FilmContentFilter.NewReleases },
            Lang
        ).Compile();

        var result = films.Where(filter).ToList();

        result.Should().HaveCount(1);
    }


    [Test]
    public void Should_Apply_Multiple_Filters_Together()
    {
        var films = Helpers.CreateFilmsWithGenres(["Action", "Comedy"]);

        var query = new Query
        {
            Genres = ["Action"],
            Content = FilmContentFilter.Popular,
            Rating = RatingEnum.ThreeStars
        };

        var filter = Specifications.BuildFilter(query, Lang).Compile();

        var result = films.Where(filter).ToList();

        result.Should().NotBeNull();
    }
}