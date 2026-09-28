using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.OkdeskEntity;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity;

public class IssueServiceMergeTests
{
    [Fact]
    public async Task UpdateIssuesFromCloudDbAsync_OlderSnapshot_DoesNotReplaceCurrentIssue()
    {
        Issue existingIssue = CreateIssue(10, 9, "Актуальная", new DateTime(2026, 9, 10, 12, 0, 0));
        existingIssue.GroupUpdatedAt = existingIssue.EmployeesUpdatedAt;
        Issue sqlIssue = CreateIssue(10, 3, "Устаревшая", existingIssue.EmployeesUpdatedAt.AddHours(-3));
        sqlIssue.GroupUpdatedAt = sqlIssue.EmployeesUpdatedAt;
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingIssue);
        IssueService service = CreateService(unitOfWork, CreateSqlSource(sqlIssue));

        await service.UpdateIssuesFromCloudDbAsync(
            new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 30),
            startIndex: 0,
            limit: 10,
            ct: TestContext.Current.CancellationToken);

        Assert.Equal(9, existingIssue.GroupId);
        Assert.Equal(new DateTime(2026, 9, 10, 12, 0, 0), existingIssue.GroupUpdatedAt);
        Assert.Equal("Актуальная", existingIssue.Title);
        Assert.Equal(new DateTime(2026, 9, 10, 12, 0, 0), existingIssue.EmployeesUpdatedAt);
    }

    [Fact]
    public async Task UpdateIssuesFromCloudDbAsync_SameVersion_FillsResponsibleGroup()
    {
        DateTime updatedAt = new(2026, 9, 10, 12, 0, 0);
        Issue existingIssue = CreateIssue(10, null, "Заявка", updatedAt);
        Issue sqlIssue = CreateIssue(10, 3, "Заявка", updatedAt);
        sqlIssue.GroupUpdatedAt = sqlIssue.EmployeesUpdatedAt;
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingIssue);
        IssueService service = CreateService(unitOfWork, CreateSqlSource(sqlIssue));

        await service.UpdateIssuesFromCloudDbAsync(
            new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 30),
            startIndex: 0,
            limit: 10,
            ct: TestContext.Current.CancellationToken);

        Assert.Equal(3, existingIssue.GroupId);
        Assert.Equal(updatedAt, existingIssue.GroupUpdatedAt);
    }

    [Fact]
    public async Task UpdateIssuesFromCloudDbAsync_OlderSnapshot_FillsGroupWhenVersionIsUnknown()
    {
        DateTime localUpdatedAt = new(2026, 5, 30, 1, 18, 41);
        DateTime sqlUpdatedAt = localUpdatedAt.AddSeconds(-4);
        Issue existingIssue = CreateIssue(10, null, "Заявка из REST", localUpdatedAt);
        Issue sqlIssue = CreateIssue(10, 11, "Старый заголовок", sqlUpdatedAt);
        sqlIssue.GroupUpdatedAt = sqlUpdatedAt;
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingIssue);
        IssueService service = CreateService(unitOfWork, CreateSqlSource(sqlIssue));

        await service.UpdateIssuesFromCloudDbAsync(
            new DateTime(2026, 5, 1),
            new DateTime(2026, 5, 31),
            startIndex: 0,
            limit: 10,
            ct: TestContext.Current.CancellationToken);

        Assert.Equal(11, existingIssue.GroupId);
        Assert.Equal(sqlUpdatedAt, existingIssue.GroupUpdatedAt);
        Assert.Equal("Заявка из REST", existingIssue.Title);
        Assert.Equal(localUpdatedAt, existingIssue.EmployeesUpdatedAt);
    }

    [Fact]
    public async Task UpdateIssuesFromCloudDbAsync_OlderSnapshot_DoesNotRestoreGroupClearedByWebhook()
    {
        DateTime webhookUpdatedAt = new(2026, 9, 10, 12, 0, 0);
        DateTime sqlUpdatedAt = webhookUpdatedAt.AddHours(-3);
        Issue existingIssue = CreateIssue(10, null, "Актуальная", webhookUpdatedAt);
        existingIssue.GroupUpdatedAt = webhookUpdatedAt;
        Issue sqlIssue = CreateIssue(10, 3, "Устаревшая", sqlUpdatedAt);
        sqlIssue.GroupUpdatedAt = sqlUpdatedAt;
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingIssue);
        IssueService service = CreateService(unitOfWork, CreateSqlSource(sqlIssue));

        await service.UpdateIssuesFromCloudDbAsync(
            new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 30),
            startIndex: 0,
            limit: 10,
            ct: TestContext.Current.CancellationToken);

        Assert.Null(existingIssue.GroupId);
        Assert.Equal(webhookUpdatedAt, existingIssue.GroupUpdatedAt);
        Assert.Equal("Актуальная", existingIssue.Title);
    }

    [Fact]
    public async Task UpdateIssuesFromCloudApiAsync_ResponseWithoutGroup_PreservesWebhookGroupAndVersion()
    {
        Issue existingIssue = CreateIssue(10, 9, "До обновления", new DateTime(2026, 9, 10, 12, 0, 0));
        existingIssue.GroupUpdatedAt = existingIssue.EmployeesUpdatedAt;
        Issue apiIssue = CreateIssue(10, null, "Из API", existingIssue.EmployeesUpdatedAt.AddHours(-1));
        IIssuesUnitOfWork unitOfWork = CreateUnitOfWork(existingIssue);
        unitOfWork.Employee.GetActiveFromIdReadOnlyAsync(
                Arg.Any<long>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Employee>()));

        IOkdeskEntityRequestService request = Substitute.For<IOkdeskEntityRequestService>();
        request.GetAllItemsAsync<Issue>(
                Arg.Any<string>(),
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<CancellationToken>())
            .Returns(SingleBatch(apiIssue));

        IssueService service = CreateService(unitOfWork, Substitute.For<IOkdeskIssuesSource>(), request);

        await service.UpdateIssuesFromCloudApiAsync(
            new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 30),
            limit: 10,
            ct: TestContext.Current.CancellationToken);

        Assert.Equal(9, existingIssue.GroupId);
        Assert.Equal(new DateTime(2026, 9, 10, 12, 0, 0), existingIssue.GroupUpdatedAt);
        Assert.Equal("Из API", existingIssue.Title);
        Assert.Equal(new DateTime(2026, 9, 10, 12, 0, 0), existingIssue.EmployeesUpdatedAt);
    }

    private static Issue CreateIssue(int id, int? groupId, string title, DateTime updatedAt) => new()
    {
        Id = id,
        GroupId = groupId,
        Title = title,
        CreatedAt = new DateTime(2026, 1, 1),
        EmployeesUpdatedAt = updatedAt
    };

    private static IIssuesUnitOfWork CreateUnitOfWork(Issue existingIssue)
    {
        IIssuesUnitOfWork unitOfWork = Substitute.For<IIssuesUnitOfWork>();
        unitOfWork.Issue.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Issue> { existingIssue }));
        return unitOfWork;
    }

    private static IOkdeskIssuesSource CreateSqlSource(Issue sqlIssue)
    {
        IOkdeskIssueRepository issueRepository = Substitute.For<IOkdeskIssueRepository>();
        issueRepository.GetUpdatedItemsAsync(
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Issue> { sqlIssue }));

        IOkdeskIssuesSource source = Substitute.For<IOkdeskIssuesSource>();
        source.Issue.Returns(issueRepository);
        return source;
    }

    private static async IAsyncEnumerable<List<Issue>> SingleBatch(Issue issue)
    {
        await Task.Yield();
        yield return new List<Issue> { issue };
    }

    private static IssueService CreateService(
        IIssuesUnitOfWork unitOfWork,
        IOkdeskIssuesSource source,
        IOkdeskEntityRequestService? request = null)
    {
        return new IssueService(
            Options.Create(new ApiEndpointOptions { OkdeskDomainUrl = "https://example.invalid" }),
            Options.Create(new OkdeskOptions { OkdeskApiToken = "test-token" }),
            request ?? Substitute.For<IOkdeskEntityRequestService>(),
            unitOfWork,
            source,
            new EntitySyncService(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            Substitute.For<ILogger<IssueService>>());
    }
}
