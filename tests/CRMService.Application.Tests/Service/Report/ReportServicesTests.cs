using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.Report;
using CRMService.Application.Service.Report;
using CRMService.Contracts.Models.Dto.Report;
using CRMService.Contracts.Models.Request;
using CRMService.Domain.Models.OkdeskEntity;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.Report;

public class ReportServicesTests
{
    [Fact]
    public async Task GetIssueDynamicsChartAsync_HourGranularity_PassesRangeAndBuildsBuckets()
    {
        IReportsUnitOfWork unitOfWork = Substitute.For<IReportsUnitOfWork>();
        DateTime from = new(2026, 1, 1, 10, 15, 0);
        DateTime to = new(2026, 1, 1, 12, 45, 0);
        unitOfWork.IssueDynamicsChartReport
            .GetCreatedIssuesAsync(from, to, "hour", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<IssueDynamicsPointInfo>
            {
                new() { BucketStart = new DateTime(2026, 1, 1, 10, 0, 0), Count = 2 },
                new() { BucketStart = new DateTime(2026, 1, 1, 10, 0, 0), Count = 3 }
            }));
        unitOfWork.IssueDynamicsChartReport
            .GetCompletedIssuesAsync(from, to, "hour", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<IssueDynamicsPointInfo>
            {
                new() { BucketStart = new DateTime(2026, 1, 1, 12, 0, 0), Count = 4 }
            }));
        IssueDynamicsChartService service = new(unitOfWork);

        IssueDynamicsChartDto result = await service.GetIssueDynamicsChartAsync(
            new IssueDynamicsChartRequest { DateFrom = from, DateTo = to, Granularity = "HOUR" },
            CancellationToken.None);

        Assert.Equal("hour", result.Granularity);
        Assert.Equal(
            new[]
            {
                new DateTime(2026, 1, 1, 10, 0, 0),
                new DateTime(2026, 1, 1, 11, 0, 0),
                new DateTime(2026, 1, 1, 12, 0, 0)
            },
            result.Buckets);
        Assert.Equal(new[] { 5, 0, 0 }, result.CreatedValues);
        Assert.Equal(new[] { 0, 0, 4 }, result.CompletedValues);
    }

    [Fact]
    public async Task GetIssueDynamicsChartAsync_RepositoryCancels_PropagatesCancellation()
    {
        IReportsUnitOfWork unitOfWork = Substitute.For<IReportsUnitOfWork>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        unitOfWork.IssueDynamicsChartReport
            .GetCreatedIssuesAsync(
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<string>(),
                cancellation.Token)
            .Returns(Task.FromCanceled<List<IssueDynamicsPointInfo>>(cancellation.Token));
        IssueDynamicsChartService service = new(unitOfWork);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetIssueDynamicsChartAsync(new IssueDynamicsChartRequest(), cancellation.Token));
    }

    [Fact]
    public async Task GetSpentTimeChart_EmployeeScope_PassesFiltersAndAggregatesSeries()
    {
        IReportsUnitOfWork unitOfWork = Substitute.For<IReportsUnitOfWork>();
        DateTime from = new(2026, 2, 1, 0, 0, 0);
        DateTime to = new(2026, 2, 2, 0, 0, 0);
        unitOfWork.Employee
            .GetByIdsReadOnlyAsync(Arg.Any<IReadOnlyCollection<int>>(), true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Employee>
            {
                new() { Id = 2, LastName = "Beta", Active = true },
                new() { Id = 1, LastName = "Alpha", Active = true }
            }));
        unitOfWork.SpentTimeChartReport
            .GetSpentTimeChartByEmployees(
                from,
                to,
                "createdAt",
                "day",
                Arg.Any<IReadOnlyCollection<int>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<TimeChartPointInfo>
            {
                new() { EntityId = 1, BucketStart = from, SpentedTime = 1.5 },
                new() { EntityId = 1, BucketStart = from, SpentedTime = 2.5 },
                new() { EntityId = 2, BucketStart = to, SpentedTime = 3 }
            }));
        SpentTimeChartService service = new(unitOfWork);
        TimeChartRequest request = new()
        {
            DateFrom = from,
            DateTo = to,
            Scope = "employee",
            TimeAxis = "createdAt",
            Granularity = "day",
            EmployeeIds = new[] { 2, 1, 1 }
        };

        TimeChartDto result = await service.GetSpentTimeChart(request, CancellationToken.None);

        Assert.Equal("employee", result.Scope);
        Assert.Equal("createdAt", result.TimeAxis);
        Assert.Equal(new[] { from, to }, result.Buckets);
        Assert.Collection(
            result.Series,
            first =>
            {
                Assert.Equal("1", first.Id);
                Assert.Equal("Alpha", first.Name);
                Assert.Equal(new[] { 4d, 0d }, first.Values);
            },
            second =>
            {
                Assert.Equal("2", second.Id);
                Assert.Equal("Beta", second.Name);
                Assert.Equal(new[] { 0d, 3d }, second.Values);
            });
        await unitOfWork.SpentTimeChartReport.Received(1).GetSpentTimeChartByEmployees(
            from,
            to,
            "createdAt",
            "day",
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.OrderBy(id => id).SequenceEqual(new[] { 1, 2 })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSpentTimeChart_NoResolvedEmployees_ReturnsEmptySeriesWithoutReportQuery()
    {
        IReportsUnitOfWork unitOfWork = Substitute.For<IReportsUnitOfWork>();
        unitOfWork.Employee.GetWithGroupsReadOnlyAsync(null, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Employee>()));
        SpentTimeChartService service = new(unitOfWork);

        TimeChartDto result = await service.GetSpentTimeChart(
            new TimeChartRequest
            {
                DateFrom = new DateTime(2026, 1, 1),
                DateTo = new DateTime(2026, 1, 1)
            },
            CancellationToken.None);

        Assert.Empty(result.Series);
        await unitOfWork.SpentTimeChartReport.DidNotReceiveWithAnyArgs()
            .GetSpentTimeChartByEmployees(
                default,
                default,
                default!,
                default!,
                default!,
                TestContext.Current.CancellationToken);
    }
}
