using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity;

public class TimeEntryServiceTests
{
    [Fact]
    public async Task CreateOrUpdate_RealtimeEntryWithoutCreatedAt_PreservesCreatedAtAndUpdatesCurrentFields()
    {
        DateTime createdAt = new(2026, 1, 10, 9, 30, 0);
        TimeEntry existingEntry = new()
        {
            Id = 10,
            EmployeeId = 1,
            IssueId = 100,
            SpentTime = 1,
            LoggedAt = new DateTime(2026, 1, 10, 8, 0, 0),
            CreatedAt = createdAt
        };
        TimeEntry incomingEntry = new()
        {
            Id = 10,
            EmployeeId = 2,
            IssueId = 100,
            SpentTime = 2.5,
            LoggedAt = new DateTime(2026, 1, 11, 12, 0, 0),
            CreatedAt = null
        };
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingEntry, incomingEntry.IssueId);
        TimeEntryService service = CreateService(unitOfWork);

        await service.CreateOrUpdate(incomingEntry, CancellationToken.None);

        Assert.Equal(2, existingEntry.EmployeeId);
        Assert.Equal(2.5, existingEntry.SpentTime);
        Assert.Equal(incomingEntry.LoggedAt, existingEntry.LoggedAt);
        Assert.Equal(createdAt, existingEntry.CreatedAt);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTimeEntriesFromCloudDb_ExistingEntry_FillsCreatedAtWithoutReplacingCurrentFields()
    {
        TimeEntry existingEntry = new()
        {
            Id = 10,
            EmployeeId = 2,
            IssueId = 100,
            SpentTime = 2.5,
            LoggedAt = new DateTime(2026, 1, 11, 12, 0, 0),
            CreatedAt = null
        };
        DateTime sqlCreatedAt = new(2026, 1, 10, 9, 30, 0);
        TimeEntry sqlEntry = new()
        {
            Id = 10,
            EmployeeId = 1,
            IssueId = 100,
            SpentTime = 1,
            LoggedAt = new DateTime(2026, 1, 10, 8, 0, 0),
            CreatedAt = sqlCreatedAt
        };
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingEntry, sqlEntry.IssueId);
        IOkdeskIssuesSource okdeskSource = CreateOkdeskSource(sqlEntry);
        TimeEntryService service = CreateService(unitOfWork, okdeskSource);

        await service.UpdateTimeEntriesFromCloudDb(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31),
            CancellationToken.None);

        Assert.Equal(2, existingEntry.EmployeeId);
        Assert.Equal(2.5, existingEntry.SpentTime);
        Assert.Equal(new DateTime(2026, 1, 11, 12, 0, 0), existingEntry.LoggedAt);
        Assert.Equal(sqlCreatedAt, existingEntry.CreatedAt);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTimeEntriesFromCloudDb_ExistingCreatedAt_DoesNotReplaceItFromDelayedSnapshot()
    {
        DateTime currentCreatedAt = new(2026, 1, 10, 9, 30, 0);
        TimeEntry existingEntry = new()
        {
            Id = 10,
            EmployeeId = 2,
            IssueId = 100,
            SpentTime = 2.5,
            LoggedAt = new DateTime(2026, 1, 11, 12, 0, 0),
            CreatedAt = currentCreatedAt
        };
        TimeEntry sqlEntry = new()
        {
            Id = 10,
            EmployeeId = 1,
            IssueId = 100,
            SpentTime = 1,
            LoggedAt = new DateTime(2026, 1, 10, 8, 0, 0),
            CreatedAt = currentCreatedAt.AddHours(-3)
        };
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingEntry, sqlEntry.IssueId);
        TimeEntryService service = CreateService(unitOfWork, CreateOkdeskSource(sqlEntry));

        await service.UpdateTimeEntriesFromCloudDb(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31),
            CancellationToken.None);

        Assert.Equal(currentCreatedAt, existingEntry.CreatedAt);
    }

    private static IIssuesUnitOfWork CreateUnitOfWork(TimeEntry existingEntry, int issueId)
    {
        IIssuesUnitOfWork unitOfWork = Substitute.For<IIssuesUnitOfWork>();
        unitOfWork.Issue.GetItemByIdReadOnlyAsync(issueId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Issue?>(new Issue { Id = issueId }));
        unitOfWork.TimeEntry.GetItemByIdAsync(existingEntry.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TimeEntry?>(existingEntry));
        return unitOfWork;
    }

    private static IOkdeskIssuesSource CreateOkdeskSource(TimeEntry sqlEntry)
    {
        IOkdeskTimeEntryRepository timeEntryRepository = Substitute.For<IOkdeskTimeEntryRepository>();
        timeEntryRepository.GetLoggedItemsAsync(
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<TimeEntry> { sqlEntry }));

        IOkdeskIssuesSource source = Substitute.For<IOkdeskIssuesSource>();
        source.TimeEntry.Returns(timeEntryRepository);
        return source;
    }

    private static TimeEntryService CreateService(
        IIssuesUnitOfWork unitOfWork,
        IOkdeskIssuesSource? okdeskSource = null)
    {
        return new TimeEntryService(
            Options.Create(new ApiEndpointOptions { OkdeskDomainUrl = "https://example.invalid" }),
            Options.Create(new OkdeskOptions { OkdeskApiToken = "test-token" }),
            Substitute.For<IOkdeskEntityRequestService>(),
            unitOfWork,
            okdeskSource ?? Substitute.For<IOkdeskIssuesSource>(),
            Substitute.For<ILogger<TimeEntryService>>());
    }
}
