using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Service.Authorization;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Authorization;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.Authorization;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUserAsync_ValidRequest_CreatesHashedUserAndSaves()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        Hasher hasher = new();
        Guid roleId = Guid.NewGuid();
        CrmRole role = new() { Id = roleId, Name = "Admin" };
        unitOfWork.User.GetByLoginReadOnlyAsync("new.user", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));
        unitOfWork.CrmRole.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<CrmRole> { role }));
        User? created = null;
        unitOfWork.User.When(repository => repository.Create(Arg.Any<User>()))
            .Do(call => created = call.Arg<User>());
        UserService service = new(unitOfWork, hasher);
        CreateUserRequest request = new()
        {
            Name = "  New User  ",
            Login = "  new.user  ",
            Password = "plain password",
            RoleIds = new List<Guid> { roleId, roleId }
        };

        ServiceResult result = await service.CreateUserAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(created);
        Assert.Equal("New User", created.Name);
        Assert.Equal("new.user", created.Login);
        Assert.True(created.Active);
        Assert.True(hasher.Verify("plain password", created.Password));
        Assert.Collection(created.Roles, actualRole => Assert.Same(role, actualRole));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateLogin_ReturnsConflictWithoutSaving()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        unitOfWork.User.GetByLoginReadOnlyAsync("existing", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(new User { Login = "existing" }));
        UserService service = new(unitOfWork, new Hasher());

        ServiceResult result = await service.CreateUserAsync(
            new CreateUserRequest
            {
                Name = "User",
                Login = "existing",
                Password = "password",
                RoleIds = new List<Guid> { Guid.NewGuid() }
            },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(409, result.Error!.StatusCode);
        unitOfWork.User.DidNotReceive().Create(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", "login", "password", "Имя обязательно.")]
    [InlineData("User", "", "password", "Логин обязателен.")]
    [InlineData("User", "login", "", "Пароль обязателен.")]
    public async Task CreateUserAsync_InvalidRequiredField_ReturnsValidationError(
        string name,
        string login,
        string password,
        string expectedMessage)
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        UserService service = new(unitOfWork, new Hasher());

        ServiceResult result = await service.CreateUserAsync(
            new CreateUserRequest { Name = name, Login = login, Password = password },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.Error!.StatusCode);
        Assert.Equal(expectedMessage, result.Error.Message);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUserAsync_ExistingUser_UpdatesFieldsRolesAndSaves()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        Hasher hasher = new();
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        User user = new()
        {
            Id = userId,
            Name = "Old",
            Login = "old",
            Password = hasher.Hash("old password"),
            Roles = new List<CrmRole> { new() { Id = Guid.NewGuid(), Name = "Old role" } }
        };
        CrmRole newRole = new() { Id = roleId, Name = "New role" };
        unitOfWork.User.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));
        unitOfWork.User.GetByLoginReadOnlyAsync("new", userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));
        unitOfWork.CrmRole.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<CrmRole> { newRole }));
        UserService service = new(unitOfWork, hasher);

        ServiceResult result = await service.UpdateUserAsync(
            new UpdateUserRequest
            {
                UserId = userId,
                Name = " New Name ",
                Login = " new ",
                Password = "new password",
                RoleIds = new List<Guid> { roleId }
            },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("New Name", user.Name);
        Assert.Equal("new", user.Login);
        Assert.True(hasher.Verify("new password", user.Password));
        Assert.Collection(user.Roles, role => Assert.Same(newRole, role));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUserAsync_UserNotFound_ReturnsNotFoundWithoutSaving()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        Guid userId = Guid.NewGuid();
        unitOfWork.User.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));
        UserService service = new(unitOfWork, new Hasher());

        ServiceResult result = await service.UpdateUserAsync(
            new UpdateUserRequest
            {
                UserId = userId,
                Name = "User",
                Login = "user",
                RoleIds = new List<Guid> { Guid.NewGuid() }
            },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.Error!.StatusCode);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateUserAsync_RepositoryCancellation_PropagatesWithoutSaving()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        unitOfWork.User.GetByLoginReadOnlyAsync("user", null, cancellation.Token)
            .Returns(Task.FromCanceled<User?>(cancellation.Token));
        UserService service = new(unitOfWork, new Hasher());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CreateUserAsync(
            new CreateUserRequest
            {
                Name = "User",
                Login = "user",
                Password = "password",
                RoleIds = new List<Guid> { Guid.NewGuid() }
            },
            cancellation.Token));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
