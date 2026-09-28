using CRMService.Application.Models.WebHook;
using CRMService.Domain.Models.OkdeskEntity;
using Xunit;

namespace CRMService.Application.Tests.Models.WebHook;

public class IssueWebHookTests
{
    [Fact]
    public void ConvertToIssue_EventAssignee_OverridesIssueSnapshotAssignee()
    {
        IssueWebHook webhook = CreateWebhook(new Employee { Id = 6 });
        AssigneeWebHook eventAssignee = new()
        {
            Employee = new Employee { Id = 5 },
            Group = new Group { Id = 3 }
        };

        Issue issue = webhook.ConvertToIssue(eventAssignee);

        Assert.Equal(5, issue.AssigneeId);
        Assert.Equal(3, issue.GroupId);
        Assert.Equal(issue.EmployeesUpdatedAt, issue.GroupUpdatedAt);
    }

    [Fact]
    public void ConvertToIssue_ExplicitEmptyEventAssignee_ClearsAssignee()
    {
        IssueWebHook webhook = CreateWebhook(new Employee { Id = 6 });

        Issue issue = webhook.ConvertToIssue(null);

        Assert.Null(issue.AssigneeId);
        Assert.Null(issue.GroupId);
        Assert.Equal(issue.EmployeesUpdatedAt, issue.GroupUpdatedAt);
    }

    private static IssueWebHook CreateWebhook(Employee assignee) => new()
    {
        Id = 10,
        Title = "Issue",
        Type = new IssueType(),
        Priority = new IssuePriority(),
        Status = new IssueStatus(),
        Author = new EmployeeWebHook { Id = 1 },
        Assignee = new AssigneeWebHook { Employee = assignee }
    };
}
