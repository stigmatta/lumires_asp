using Ardalis.Result;
using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmPeople.GetActorStats;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;
using Moq;

namespace Tests.ApiTests.People;

internal sealed class GetActorStatsTests
{
    private Mock<IExternalAwardsService> _awardsMock = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IExternalFilmService> _filmServiceMock = null!;
    private Mock<IPersonResolver> _resolverMock = null!;

    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.LangCulture).Returns("en");

        _resolverMock = new Mock<IPersonResolver>();
        _resolverMock
            .Setup(x => x.EnsurePersonExistsAsync(
                It.IsAny<(int, string)>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _filmServiceMock = new Mock<IExternalFilmService>();
        _awardsMock = new Mock<IExternalAwardsService>();
    }

    private static ExternalFilmShort Film(int id, float voteAverage, int voteCount) =>
        new(id, $"Film {id}", null, 2000, voteAverage, voteCount, 1f, []);

    private Endpoint CreateEndpoint() =>
        Factory.Create<Endpoint>(
            _currentUserMock.Object,
            _filmServiceMock.Object,
            _awardsMock.Object,
            _resolverMock.Object);

    private void SetupActorFilms(int personId, params ExternalFilmShort[] films) =>
        _filmServiceMock
            .Setup(x => x.GetPersonCreditsAsync(personId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ExternalPersonCredits(personId, films, [])));

    [Test]
    public async Task Should_Return_FilmCount_And_AverageRating_Of_Rated_Films()
    {
        // Arrange — TMDB ratings 8.0 and 6.0 -> avg 7.0 -> 3.5 on the 0–5 scale;
        // the 0-vote film is excluded from the average but counted in FilmsCount
        const int personId = 287;
        SetupActorFilms(personId,
            Film(1, 8.0f, 100),
            Film(2, 6.0f, 50),
            Film(3, 0f, 0));

        _awardsMock
            .Setup(x => x.GetPersonAwardsAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PersonAwards(9, 3)));

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(personId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.FilmsCount.Should().Be(3);
        ep.Response.AverageRating.Should().Be(3.5);
        ep.Response.Awards.Should().NotBeNull();
        ep.Response.Awards!.Nominations.Should().Be(9);
        ep.Response.Awards.Wins.Should().Be(3);
    }

    [Test]
    public async Task Should_Return_Null_Awards_When_Scraping_Fails()
    {
        // Arrange
        const int personId = 287;
        SetupActorFilms(personId, Film(1, 7.0f, 10));

        _awardsMock
            .Setup(x => x.GetPersonAwardsAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error("boom"));

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(personId), CancellationToken.None);

        // Assert — film stats still returned, awards null
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.FilmsCount.Should().Be(1);
        ep.Response.Awards.Should().BeNull();
    }

    [Test]
    public async Task Should_Return_Zero_AverageRating_When_No_Rated_Films()
    {
        // Arrange — single film with no votes
        const int personId = 287;
        SetupActorFilms(personId, Film(1, 0f, 0));

        _awardsMock
            .Setup(x => x.GetPersonAwardsAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PersonAwards(0, 0)));

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(personId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.FilmsCount.Should().Be(1);
        ep.Response.AverageRating.Should().Be(0);
    }

    [Test]
    public async Task Should_Propagate_Error_When_Credits_Fetch_Fails()
    {
        // Arrange
        const int personId = 999;
        _filmServiceMock
            .Setup(x => x.GetPersonCreditsAsync(personId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(personId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }
}
