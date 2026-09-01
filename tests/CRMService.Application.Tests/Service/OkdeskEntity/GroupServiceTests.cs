using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity;

public class GroupServiceTests
{
    [Fact]
    public async Task UpsertEmployeeGroupConnectionsFromApi_EmployeeRemovedFromLastGroup_DeletesStaleConnection()
    {
        ICompanyDirectoryUnitOfWork unitOfWork = Substitute.For<ICompanyDirectoryUnitOfWork>();
        IOkdeskCompanyDirectorySource source = Substitute.For<IOkdeskCompanyDirectorySource>();
        IOkdeskEntityRequestService request = Substitute.For<IOkdeskEntityRequestService>();
        Group group = new() { Id = 7, Name = "L1", Employees = [] };
        EmployeeGroup staleConnection = new() { EmployeeId = 42, GroupId = group.Id };

        request.GetRangeOfItemsAsync<Group>(
                Arg.Any<string>(),
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Group> { group }));
        unitOfWork.EmployeeGroup
            .GetByGroupIdsReadOnlyAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<EmployeeGroup> { staleConnection }));

        GroupService service = new(
            Options.Create(new ApiEndpointOptions { OkdeskDomainUrl = "https://okdesk.invalid" }),
            Options.Create(new OkdeskOptions { OkdeskApiToken = "token" }),
            request,
            unitOfWork,
            source,
            new EntitySyncService(),
            NullLogger<GroupService>.Instance);

        await service.UpsertEmployeeGroupConnectionsFromApi(TestContext.Current.CancellationToken);

        unitOfWork.EmployeeGroup.Received(1).DeleteRange(
            Arg.Is<IEnumerable<EmployeeGroup>>(items => items.Single() == staleConnection));
        await unitOfWork.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
