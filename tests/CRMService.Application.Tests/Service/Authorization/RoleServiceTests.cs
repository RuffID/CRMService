using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Service.Authorization;
using CRMService.Contracts.Models.Dto.Authorization;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Authorization;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.Authorization;

public class RoleServiceTests
{
    [Fact]
    public async Task GetRolesAsync_RepositoryReturnsRoles_MapsResultAndForwardsCancellation()
    {
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        using CancellationTokenSource cancellation = new();
        Guid roleId = Guid.NewGuid();
        unitOfWork.CrmRole.GetItemsReadOnlyAsync(cancellation.Token)
            .Returns(Task.FromResult(new List<CrmRole> { new() { Id = roleId, Name = "Admin" } }));
        RoleService service = new(unitOfWork);

        ServiceResult<List<CrmRoleDto>> result = await service.GetRolesAsync(cancellation.Token);

        Assert.True(result.Success);
        Assert.Collection(result.Data!, role =>
        {
            Assert.Equal(roleId, role.Id);
            Assert.Equal("Admin", role.Name);
        });
    }
}
