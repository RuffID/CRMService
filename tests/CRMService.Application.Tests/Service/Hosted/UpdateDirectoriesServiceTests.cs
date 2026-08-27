using CRMService.Application.Abstractions.Service;
using CRMService.Application.Service.Hosted;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.Hosted;

public class UpdateDirectoriesServiceTests
{
    [Fact]
    public async Task RunUpdateDirectories_AllOperationsSucceed_UsesDependencyOrder()
    {
        IDirectoryUpdateOperations operations = Substitute.For<IDirectoryUpdateOperations>();
        UpdateDirectoriesService service = new(operations, Substitute.For<ILoggerFactory>());

        await service.RunUpdateDirectories(CancellationToken.None);

        string[] calls = operations.ReceivedCalls()
            .Select(call => call.GetMethodInfo().Name)
            .ToArray();
        Assert.Equal(ExpectedOrder, calls);
    }

    [Fact]
    public async Task RunUpdateDirectories_OperationFails_StopsAtPrimaryError()
    {
        IDirectoryUpdateOperations operations = Substitute.For<IDirectoryUpdateOperations>();
        operations.UpdateManufacturersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("manufacturer update failed")));
        UpdateDirectoriesService service = new(operations, Substitute.For<ILoggerFactory>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RunUpdateDirectories(CancellationToken.None));

        string[] calls = operations.ReceivedCalls()
            .Select(call => call.GetMethodInfo().Name)
            .ToArray();
        Assert.Equal(ExpectedOrder.Take(4), calls);
    }

    private static readonly string[] ExpectedOrder =
    {
        nameof(IDirectoryUpdateOperations.UpdateKindsAsync),
        nameof(IDirectoryUpdateOperations.UpdateKindParametersAsync),
        nameof(IDirectoryUpdateOperations.UpdateKindParameterConnectionsAsync),
        nameof(IDirectoryUpdateOperations.UpdateManufacturersAsync),
        nameof(IDirectoryUpdateOperations.UpdateModelsAsync),
        nameof(IDirectoryUpdateOperations.EnsureAnonymousCategoryAsync),
        nameof(IDirectoryUpdateOperations.UpdateCategoriesAsync),
        nameof(IDirectoryUpdateOperations.UpdateCompaniesAsync),
        nameof(IDirectoryUpdateOperations.UpdateMaintenanceEntitiesAsync),
        nameof(IDirectoryUpdateOperations.UpdateRolesAsync),
        nameof(IDirectoryUpdateOperations.UpdateGroupsAsync),
        nameof(IDirectoryUpdateOperations.UpdateEmployeesAsync),
        nameof(IDirectoryUpdateOperations.UpdateEmployeeGroupConnectionsAsync),
        nameof(IDirectoryUpdateOperations.UpdateEmployeeRoleConnectionsAsync),
        nameof(IDirectoryUpdateOperations.UpdateIssuePrioritiesAsync),
        nameof(IDirectoryUpdateOperations.UpdateIssueTypesAsync),
        nameof(IDirectoryUpdateOperations.UpdateIssueStatusesAsync)
    };
}
