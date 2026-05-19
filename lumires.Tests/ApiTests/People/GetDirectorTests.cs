using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.FilmPeople.GetDirector;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.People;

internal sealed class GetDirectorTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
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

        _dbContextMock = new Mock<IAppDbContext>();
        _dbContextMock
            .Setup(x => x.PersonsDetails)
            .Returns(new List<PersonDetail>().BuildMockDbSet().Object);

        _dataAccess = new DataAccess(_dbContextMock.Object);
    }

    // Creates a PersonDetail with the Person navigation property set —
    // required because DataAccess queries pd.Person.ExternalId and MockQueryable
    // runs LINQ in-memory where EF navigation properties are null by default.
    private static PersonDetail CreateDetail(Person person, string lang,
        string biography, DateOnly birthday, DateOnly? deathday,
        GenderType gender, string placeOfBirth, string profilePath)
    {
        var detail = new PersonDetail(person.Id, lang, biography, birthday, deathday, gender, placeOfBirth, profilePath);
        detail.SetPerson(person);
        return detail;
    }

    private Endpoint CreateEndpoint(DataAccess? dataAccess = null, IPersonResolver? resolver = null)
    {
        return Factory.Create<Endpoint>(
            resolver ?? _resolverMock.Object,
            _currentUserMock.Object,
            dataAccess ?? _dataAccess);
    }

    [Test]
    [Arguments(1)]
    [Arguments(0)]
    public async Task GetDirector_Should_Be_404_When_NotFound(int tmdbId)
    {
        // Arrange — empty DB, no detail row exists
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(tmdbId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(404);
    }

    [Test]
    [Arguments(1, "Christopher Nolan bio", "1970-07-30", null, GenderType.Male, "London, UK", "/nolan.jpg")]
    [Arguments(42, "Denis Villeneuve bio", "1967-10-03", null, GenderType.Male, "Gentilly, Canada", "/villeneuve.jpg")]
    public async Task GetDirector_Should_Be_200_When_FoundInDb(
        int externalId, string biography, string birthdayStr, string? deathdayStr,
        GenderType gender, string placeOfBirth, string profilePath)
    {
        // Arrange
        var birthday = DateOnly.Parse(birthdayStr);
        var deathday = deathdayStr is not null ? DateOnly.Parse(deathdayStr) : (DateOnly?)null;

        var person = new Person(externalId, PersonDepartment.Directing);
        var detail = CreateDetail(person, "en", biography, birthday, deathday, gender, placeOfBirth, profilePath);

        _dbContextMock
            .Setup(x => x.PersonsDetails)
            .Returns(new List<PersonDetail> { detail }.BuildMockDbSet().Object);

        var ep = CreateEndpoint(new DataAccess(_dbContextMock.Object));

        // Act
        await ep.HandleAsync(new Query(externalId), CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Biography.Should().Be(biography);
        ep.Response.Birthday.Should().Be(birthday);
        ep.Response.ProfilePath.Should().Be(profilePath);
    }

    [Test]
    [Arguments(2)]
    [Arguments(500)]
    public async Task GetDirector_Should_CallResolver_ForEveryRequest(int id)
    {
        // Arrange
        var ep = CreateEndpoint();

        // Act
        await ep.HandleAsync(new Query(id), CancellationToken.None);

        // Assert
        _resolverMock.Verify(
            x => x.EnsurePersonExistsAsync(
                It.Is<(int externalId, string)>(t => t.externalId == id),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Arguments(1, "Christopher Nolan bio", "1970-07-30", GenderType.Male, "London, UK", "/nolan.jpg")]
    [Arguments(42, "Denis Villeneuve bio", "1967-10-03", GenderType.Male, "Gentilly, Canada", "/villeneuve.jpg")]
    public async Task GetDirector_Should_ReturnExactLang_When_BothLangsAvailable(
        int externalId, string biography, string birthdayStr,
        GenderType gender, string placeOfBirth, string profilePath)
    {
        // Arrange — two detail rows exist: en (default) and uk-UA
        var birthday = DateOnly.Parse(birthdayStr);
        var person = new Person(externalId, PersonDepartment.Directing);

        var enDetail = CreateDetail(person, "en", biography + " EN", birthday, null, gender, placeOfBirth, profilePath);
        var uaDetail = CreateDetail(person, "uk-UA", biography + " UA", birthday, null, gender, placeOfBirth, profilePath);

        _dbContextMock
            .Setup(x => x.PersonsDetails)
            .Returns(new List<PersonDetail> { enDetail, uaDetail }.BuildMockDbSet().Object);

        var uaUserMock = new Mock<ICurrentUserService>();
        uaUserMock.Setup(x => x.LangCulture).Returns("uk-UA");

        var ep = Factory.Create<Endpoint>(
            _resolverMock.Object,
            uaUserMock.Object,
            new DataAccess(_dbContextMock.Object));

        // Act
        await ep.HandleAsync(new Query(externalId), CancellationToken.None);

        // Assert — the uk-UA row is preferred over the fallback
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Lang.Should().Be("uk-UA");
        ep.Response.Biography.Should().Be(biography + " UA");
    }

    [Test]
    [Arguments(1, "Christopher Nolan bio", "1970-07-30", GenderType.Male, "London, UK", "/nolan.jpg")]
    public async Task GetDirector_Should_FallbackToDefaultLang_When_RequestedLangMissing(
        int externalId, string biography, string birthdayStr,
        GenderType gender, string placeOfBirth, string profilePath)
    {
        // Arrange — only the default lang (en) row exists; user requests uk-UA
        var birthday = DateOnly.Parse(birthdayStr);
        var person = new Person(externalId, PersonDepartment.Directing);
        var enDetail = CreateDetail(person, "en-US", biography, birthday, null, gender, placeOfBirth, profilePath);

        _dbContextMock
            .Setup(x => x.PersonsDetails)
            .Returns(new List<PersonDetail> { enDetail }.BuildMockDbSet().Object);

        var uaUserMock = new Mock<ICurrentUserService>();
        uaUserMock.Setup(x => x.LangCulture).Returns("uk-UA");

        var ep = Factory.Create<Endpoint>(
            _resolverMock.Object,
            uaUserMock.Object,
            new DataAccess(_dbContextMock.Object));

        // Act
        await ep.HandleAsync(new Query(externalId), CancellationToken.None);

        // Assert — falls back to the en row
        ep.HttpContext.Response.StatusCode.Should().Be(200);
        ep.Response.Biography.Should().Be(biography);
    }
}