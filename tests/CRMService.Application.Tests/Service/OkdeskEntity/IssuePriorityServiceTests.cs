using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity;

public class IssuePriorityServiceTests
{
    [Fact]
    public async Task GetIssuePriorityLookupAsync_ValidRequest_NormalizesOrdersAndPages()
    {
        IIssuesUnitOfWork unitOfWork = Substitute.For<IIssuesUnitOfWork>();
        unitOfWork.IssuePriority.SearchReadOnlyAsync("hi", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<IssuePriority>
            {
                new() { Id = 3, Name = "Low" },
                new() { Id = 2, Name = "Very high" },
                new() { Id = 1, Name = "High" }
            }));
        IssuePriorityService service = CreateService(unitOfWork);

        ServiceResult<List<LookupOptionDto>> result = await service.GetIssuePriorityLookupAsync(
            new LookupListRequest { Search = " hi ", Offset = 0, Limit = 2 },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Collection(
            result.Data!,
            first =>
            {
                Assert.Equal(1, first.Id);
                Assert.Equal("High", first.Text);
            },
            second =>
            {
                Assert.Equal(3, second.Id);
                Assert.Equal("Low", second.Text);
            });
    }

    [Fact]
    public async Task GetIssuePriorityLookupAsync_InvalidOffset_ReturnsValidationErrorWithoutQuery()
    {
        IIssuesUnitOfWork unitOfWork = Substitute.For<IIssuesUnitOfWork>();
        IssuePriorityService service = CreateService(unitOfWork);

        ServiceResult<List<LookupOptionDto>> result = await service.GetIssuePriorityLookupAsync(
            new LookupListRequest { Offset = -1, Limit = 20 },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.Error!.StatusCode);
        await unitOfWork.IssuePriority.DidNotReceive()
            .SearchReadOnlyAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateIssuePrioritiesFromCloudApi_NewPriority_CreatesAndSaves()
    {
        IIssuesUnitOfWork unitOfWork = Substitute.For<IIssuesUnitOfWork>();
        IOkdeskEntityRequestService request = Substitute.For<IOkdeskEntityRequestService>();
        IssuePriority priority = new() { Id = 99, Code = "high", Name = "High" };
        request.GetRangeOfItemsAsync<IssuePriority>(Arg.Any<string>(), ct: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<IssuePriority> { priority }));
        unitOfWork.IssuePriority.GetByCodeAsync("high", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IssuePriority?>(null));
        IssuePriorityService service = CreateService(unitOfWork, request);

        await service.UpdateIssuePrioritiesFromCloudApi(CancellationToken.None);

        Assert.Equal(0, priority.Id);
        unitOfWork.IssuePriority.Received(1).Create(priority);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetIssuePrioritiesAsync_RepositoryCancels_PropagatesCancellation()
    {
        IIssuesUnitOfWork unitOfWork = Substitute.For<IIssuesUnitOfWork>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        unitOfWork.IssuePriority.GetItemsReadOnlyAsync(cancellation.Token)
            .Returns(Task.FromCanceled<List<IssuePriority>>(cancellation.Token));
        IssuePriorityService service = CreateService(unitOfWork);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetIssuePrioritiesAsync(cancellation.Token));
    }

    private static IssuePriorityService CreateService(
        IIssuesUnitOfWork unitOfWork,
        IOkdeskEntityRequestService? request = null)
    {
        return new IssuePriorityService(
            Options.Create(new ApiEndpointOptions { OkdeskDomainUrl = "https://example.invalid" }),
            Options.Create(new OkdeskOptions { OkdeskApiToken = "test-token" }),
            request ?? Substitute.For<IOkdeskEntityRequestService>(),
            unitOfWork,
            Substitute.For<IOkdeskIssuesSource>(),
            new EntitySyncService(),
            Substitute.For<ILogger<IssuePriorityService>>());
    }
}
