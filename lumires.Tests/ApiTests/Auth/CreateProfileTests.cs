using FastEndpoints;
using FluentAssertions;
using lumires.Api.Features.Auth.Commands.CreateProfile;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MockQueryable.Moq;
using Moq;

namespace Tests.ApiTests.Auth;

internal sealed class CreateProfileEndpointTests
{
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private DataAccess _dataAccess = null!;
    private Mock<IAppDbContext> _dbContextMock = null!;
    private Validator _validator = null!;


    [Before(Test)]
    public void Setup()
    {
        _currentUserMock = new Mock<ICurrentUserService>();

        _dbContextMock = new Mock<IAppDbContext>();
        _dataAccess = new DataAccess(_dbContextMock.Object);
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        localizer.Setup(x => x[It.IsAny<string>()])
            .Returns<string>(key => new LocalizedString(key, key));

        _validator = new Validator(localizer.Object);
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
    public async Task HandleAsync_WhenClaimsMismatch_ReturnsUnauthorized()
    {
        // Arrange
        var guid = Guid.NewGuid();
        const string email = "default@gmail.com";
        _currentUserMock.Setup(x => x.UserId).Returns(guid);
        _currentUserMock.Setup(x => x.Email).Returns(email);

        var ep = Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);

        var command = new Command(Guid.NewGuid(), "username", "other@email.com");

        // Act
        await ep.HandleAsync(command, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(401);
    }

    [Test]
    public async Task HandleAsync_WhenUserAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var guid = Guid.NewGuid();
        const string email = "default@gmail.com";
        _currentUserMock.Setup(x => x.UserId).Returns(guid);
        _currentUserMock.Setup(x => x.Email).Returns(email);

        var users = new List<User>
        {
            new(guid, "user", email)
        }.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Users).Returns(users.Object);

        var ep = Factory.Create<Endpoint>(_currentUserMock.Object, _dataAccess);
        var command = new Command(guid, "username", email);

        // Act
        await ep.HandleAsync(command, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(409);
    }

    [Test]
    public async Task HandleAsync_WhenValid_ReturnsCreated()
    {
        // Arrange
        var guid = Guid.NewGuid();
        const string email = "default@gmail.com";
        _currentUserMock.Setup(x => x.UserId).Returns(guid);
        _currentUserMock.Setup(x => x.Email).Returns(email);

        var users = new List<User>().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Users).Returns(users.Object);
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var ep = CreateEndpoint();

        var command = new Command(guid, "username", email);

        // Act
        await ep.HandleAsync(command, CancellationToken.None);

        // Assert
        ep.HttpContext.Response.StatusCode.Should().Be(201);
    }

    // Username
    [Test]
    [Arguments("validUser")]
    [Arguments("user_123")]
    [Arguments("ABC")]
    public async Task Username_WhenValid_PassesValidation(string username)
    {
        var command = new Command(Guid.NewGuid(), username, "test@email.com");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Test]
    [Arguments("")]
    [Arguments("  ")]
    public async Task Username_WhenEmpty_FailsValidation(string username)
    {
        var command = new Command(Guid.NewGuid(), username, "test@email.com");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(Command.Username));
    }

    [Test]
    [Arguments("ab")] // too short
    [Arguments("thisusernameistoolong123")] // too long
    [Arguments("invalid user")] // space
    [Arguments("invalid@user")] // special char
    public async Task Username_WhenInvalid_FailsValidation(string username)
    {
        var command = new Command(Guid.NewGuid(), username, "test@email.com");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(Command.Username));
    }

    // Email
    [Test]
    [Arguments("user@email.com")]
    [Arguments("user.name@domain.org")]
    [Arguments("user-name@sub.domain.com")]
    public async Task Email_WhenValid_PassesValidation(string email)
    {
        var command = new Command(Guid.NewGuid(), "validUser", email);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Test]
    [Arguments("")]
    [Arguments("  ")]
    public async Task Email_WhenEmpty_FailsValidation(string email)
    {
        var command = new Command(Guid.NewGuid(), "validUser", email);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(Command.Email));
    }

    [Test]
    [Arguments("notanemail")]
    [Arguments("missing@domain")]
    [Arguments("@nodomain.com")]
    [Arguments("spaces in@email.com")]
    public async Task Email_WhenInvalid_FailsValidation(string email)
    {
        var command = new Command(Guid.NewGuid(), "validUser", email);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(Command.Email));
    }
}