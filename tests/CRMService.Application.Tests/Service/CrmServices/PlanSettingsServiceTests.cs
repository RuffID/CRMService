using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.CrmServices;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.CrmEntities;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.CrmEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.CrmServices;

public class PlanSettingsServiceTests
{
    [Fact]
    public async Task GetPlans_RepositoryData_SortsAndNormalizesPeriods()
    {
        IPlanSettingsUnitOfWork unitOfWork = Substitute.For<IPlanSettingsUnitOfWork>();
        unitOfWork.Plan.GetItemsReadOnlyAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Plan>
            {
                new() { Id = Guid.NewGuid(), Name = "Zulu", Period = "DAY" },
                new() { Id = Guid.NewGuid(), Name = "Alpha", Period = "invalid" }
            }));
        PlanSettingsService service = CreateService(unitOfWork);

        ServiceResult<List<PlanDto>> result = await service.GetPlans(CancellationToken.None);

        Assert.True(result.Success);
        Assert.Collection(
            result.Data!,
            first =>
            {
                Assert.Equal("Alpha", first.Name);
                Assert.Equal("month", first.Period);
            },
            second =>
            {
                Assert.Equal("Zulu", second.Name);
                Assert.Equal("day", second.Period);
            });
    }

    [Fact]
    public async Task SavePlans_MixedChanges_UpdatesCreatesDeletesAndSavesOnce()
    {
        IPlanSettingsUnitOfWork unitOfWork = Substitute.For<IPlanSettingsUnitOfWork>();
        Guid keepId = Guid.NewGuid();
        Plan kept = new() { Id = keepId, Name = "Old", Period = "month" };
        Plan deleted = new() { Id = Guid.NewGuid(), Name = "Delete", Period = "month" };
        unitOfWork.Plan.GetItemsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Plan> { kept, deleted }));
        Plan? created = null;
        unitOfWork.Plan.When(repository => repository.Create(Arg.Any<Plan>()))
            .Do(call => created = call.Arg<Plan>());
        PlanSettingsService service = CreateService(unitOfWork);

        ServiceResult<bool> result = await service.SavePlans(
            new List<PlanDto>
            {
                new() { Id = keepId, Name = " Updated ", PlanColor = " #aabbcc ", Period = "WEEK" },
                new() { Name = "Created", PlanColor = null, Period = "year" }
            },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Updated", kept.Name);
        Assert.Equal("#AABBCC", kept.PlanColor);
        Assert.Equal("week", kept.Period);
        unitOfWork.Plan.Received(1).Delete(deleted);
        Assert.NotNull(created);
        Assert.Equal("Created", created.Name);
        Assert.Equal("year", created.Period);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavePlans_DuplicateNames_ReturnsValidationErrorWithoutRepositoryCalls()
    {
        IPlanSettingsUnitOfWork unitOfWork = Substitute.For<IPlanSettingsUnitOfWork>();
        PlanSettingsService service = CreateService(unitOfWork);

        ServiceResult<bool> result = await service.SavePlans(
            new List<PlanDto>
            {
                new() { Name = "Plan", Period = "month" },
                new() { Name = " plan ", Period = "day" }
            },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.Error!.StatusCode);
        await unitOfWork.Plan.DidNotReceive().GetItemsAsync(Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveGeneralSettings_MissingSettings_CreatesDefaultThenSavesUpdatedValue()
    {
        IPlanSettingsUnitOfWork unitOfWork = Substitute.For<IPlanSettingsUnitOfWork>();
        unitOfWork.GeneralSettings.GetItemsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<GeneralSettings>()));
        GeneralSettings? created = null;
        unitOfWork.GeneralSettings.When(repository => repository.Create(Arg.Any<GeneralSettings>()))
            .Do(call => created = call.Arg<GeneralSettings>());
        PlanSettingsService service = CreateService(unitOfWork);

        ServiceResult<bool> result = await service.SaveGeneralSettings(
            new GeneralSettingsDto { PlanSwitchSeconds = 25 },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(created);
        Assert.Equal(25, created.PlanSwitchSeconds);
        await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavePlanSettings_MissingPlan_ReturnsValidationErrorWithoutSaving()
    {
        IPlanSettingsUnitOfWork unitOfWork = Substitute.For<IPlanSettingsUnitOfWork>();
        Guid planId = Guid.NewGuid();
        unitOfWork.Plan.GetByIdsReadOnlyAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Plan>()));
        PlanSettingsService service = CreateService(unitOfWork);

        ServiceResult<bool> result = await service.SavePlanSettings(
            new List<PlanSettingDto>
            {
                new() { PlanId = planId, EmployeeId = 5, PlanValue = 10 }
            },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.Error!.StatusCode);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavePlanColorSchemes_OverlappingRanges_ReturnsValidationErrorBeforeLookup()
    {
        IPlanSettingsUnitOfWork unitOfWork = Substitute.For<IPlanSettingsUnitOfWork>();
        Guid planId = Guid.NewGuid();
        PlanSettingsService service = CreateService(unitOfWork);

        ServiceResult<bool> result = await service.SavePlanColorSchemes(
            planId,
            new List<PlanColorSchemeDto>
            {
                new() { PlanId = planId, FromPercent = 0, ToPercent = 50, Color = "#000000" },
                new() { PlanId = planId, FromPercent = 50, ToPercent = 100, Color = "#FFFFFF" }
            },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.Error!.StatusCode);
        await unitOfWork.Plan.DidNotReceive().GetItemByIdReadOnlyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPlans_RepositoryCancellation_PropagatesCancellation()
    {
        IPlanSettingsUnitOfWork unitOfWork = Substitute.For<IPlanSettingsUnitOfWork>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        unitOfWork.Plan.GetItemsReadOnlyAsync(cancellation.Token)
            .Returns(Task.FromCanceled<List<Plan>>(cancellation.Token));
        PlanSettingsService service = CreateService(unitOfWork);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetPlans(cancellation.Token));
    }

    private static PlanSettingsService CreateService(IPlanSettingsUnitOfWork unitOfWork)
    {
        EmployeeService employeeService = new(
            Options.Create(new ApiEndpointOptions()),
            Options.Create(new OkdeskOptions()),
            Substitute.For<ICompanyDirectoryUnitOfWork>(),
            Substitute.For<IOkdeskCompanyDirectorySource>(),
            Substitute.For<IOkdeskEntityRequestService>(),
            new EntitySyncService(),
            Substitute.For<ILogger<EmployeeService>>());

        return new PlanSettingsService(unitOfWork, employeeService);
    }
}
