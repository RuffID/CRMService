using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.Authorization;
using CRMService.Application.Tests.Builders;
using CRMService.Domain.Models.Authorization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.Authorization;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ActiveUserWithCorrectPassword_ReturnsUser()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        Hasher hasher = new();
        User user = new UserBuilder().WithPassword(hasher.Hash("password")).Build();
        unitOfWork.User.GetByLoginWithRolesReadOnlyAsync("test.user", true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));
        AuthenticationService service = CreateService(unitOfWork, hasher);

        User? result = await service.AuthenticateAsync("test.user", "password", true, CancellationToken.None);

        Assert.Same(user, result);
    }

    [Fact]
    public async Task AuthenticateAsync_InactiveUser_ReturnsNullWithoutPasswordVerificationFailure()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        User user = new UserBuilder().Inactive().WithPassword("damaged hash").Build();
        unitOfWork.User.GetByLoginWithRolesReadOnlyAsync("test.user", false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));
        AuthenticationService service = CreateService(unitOfWork, new Hasher());

        User? result = await service.AuthenticateAsync("test.user", "password", false, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_CreatesSessionAndSaves()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        Hasher hasher = new();
        User user = new UserBuilder().WithPassword(hasher.Hash("password")).Build();
        unitOfWork.User.GetByLoginWithRolesReadOnlyAsync("test.user", false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));
        IAccessTokenService accessToken = Substitute.For<IAccessTokenService>();
        accessToken.Create(user).Returns("access-token");
        IRandomStringGenerator random = Substitute.For<IRandomStringGenerator>();
        random.GetBase64RandomString().Returns("refresh-token");
        Session? createdSession = null;
        unitOfWork.Session.When(repository => repository.Create(Arg.Any<Session>()))
            .Do(call => createdSession = call.Arg<Session>());
        AuthenticationService service = CreateService(unitOfWork, hasher, accessToken, random);

        Token? result = await service.LoginAsync("test.user", "password", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.NotNull(createdSession);
        Assert.Equal(user.Id, createdSession.UserId);
        Assert.Equal("refresh-token", createdSession.RefreshToken);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsNullWithoutSaving()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        unitOfWork.User.GetByLoginWithRolesReadOnlyAsync("missing", false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));
        AuthenticationService service = CreateService(unitOfWork, new Hasher());

        Token? result = await service.LoginAsync("missing", "password", CancellationToken.None);

        Assert.Null(result);
        unitOfWork.Session.DidNotReceive().Create(Arg.Any<Session>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTokensAsync_MissingSession_ReturnsNullWithoutSaving()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        unitOfWork.Session.GetByRefreshTokenAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Session?>(null));
        AuthenticationService service = CreateService(unitOfWork, new Hasher());

        Token? result = await service.UpdateTokensAsync("missing", CancellationToken.None);

        Assert.Null(result);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static AuthenticationService CreateService(
        IAuthorizationUnitOfWork unitOfWork,
        Hasher hasher,
        IAccessTokenService? accessToken = null,
        IRandomStringGenerator? random = null)
    {
        return new AuthenticationService(
            unitOfWork,
            hasher,
            accessToken ?? Substitute.For<IAccessTokenService>(),
            random ?? Substitute.For<IRandomStringGenerator>(),
            Substitute.For<ILogger<AuthenticationService>>());
    }
}
