using CRMService.Domain.Models.CrmEntities;
using CRMService.Domain.Models.OkdeskEntity;
using Xunit;

namespace CRMService.Domain.Tests.Models;

public class DomainCopyAndComparisonTests
{
    [Fact]
    public void CopyData_Issue_CopiesScalarStateAndPreservesIdentityAndRelationships()
    {
        Employee originalAssignee = new() { Id = 1 };
        List<TimeEntry> originalEntries = new() { new TimeEntry { Id = 2 } };
        Issue target = new()
        {
            Id = 100,
            Assignee = originalAssignee,
            TimeEntries = originalEntries
        };
        Issue source = new()
        {
            Id = 200,
            AssigneeId = 3,
            GroupId = 12,
            AuthorId = 4,
            Title = "Updated issue",
            EmployeesUpdatedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            CreatedAt = new DateTime(2025, 2, 3, 4, 5, 6, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc),
            DeadlineAt = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc),
            DelayTo = new DateTime(2026, 4, 5, 6, 7, 8, DateTimeKind.Utc),
            DeletedAt = new DateTime(2026, 5, 6, 7, 8, 9, DateTimeKind.Utc),
            StatusId = 5,
            TypeId = 6,
            PriorityId = 7,
            CompanyId = 8,
            ServiceObjectId = 9,
            Assignee = new Employee { Id = 10 },
            TimeEntries = new List<TimeEntry> { new TimeEntry { Id = 11 } }
        };

        target.CopyData(source);

        Assert.Equal(100, target.Id);
        Assert.Equal(source.AssigneeId, target.AssigneeId);
        Assert.Equal(source.GroupId, target.GroupId);
        Assert.Equal(source.AuthorId, target.AuthorId);
        Assert.Equal(source.Title, target.Title);
        Assert.Equal(source.EmployeesUpdatedAt, target.EmployeesUpdatedAt);
        Assert.Equal(source.CreatedAt, target.CreatedAt);
        Assert.Equal(source.CompletedAt, target.CompletedAt);
        Assert.Equal(source.DeadlineAt, target.DeadlineAt);
        Assert.Equal(source.DelayTo, target.DelayTo);
        Assert.Equal(source.DeletedAt, target.DeletedAt);
        Assert.Equal(source.StatusId, target.StatusId);
        Assert.Equal(source.TypeId, target.TypeId);
        Assert.Equal(source.PriorityId, target.PriorityId);
        Assert.Equal(source.CompanyId, target.CompanyId);
        Assert.Equal(source.ServiceObjectId, target.ServiceObjectId);
        Assert.Same(originalAssignee, target.Assignee);
        Assert.Same(originalEntries, target.TimeEntries);
        Assert.NotSame(source.TimeEntries, target.TimeEntries);
    }

    [Fact]
    public void CopyData_Plan_CopiesScalarStateAndPreservesMutableCollections()
    {
        List<PlanSetting> originalSettings = new() { new PlanSetting() };
        List<PlanColorScheme> originalSchemes = new() { new PlanColorScheme() };
        Plan target = new()
        {
            Id = Guid.NewGuid(),
            PlanSettings = originalSettings,
            PlanColorSchemes = originalSchemes
        };
        Plan source = new()
        {
            Id = Guid.NewGuid(),
            Name = "Monthly plan",
            PlanColor = "#102030",
            Period = "month",
            PlanSettings = new List<PlanSetting> { new PlanSetting() },
            PlanColorSchemes = new List<PlanColorScheme> { new PlanColorScheme() }
        };
        Guid targetId = target.Id;

        target.CopyData(source);

        Assert.Equal(targetId, target.Id);
        Assert.Equal(source.Name, target.Name);
        Assert.Equal(source.PlanColor, target.PlanColor);
        Assert.Equal(source.Period, target.Period);
        Assert.Same(originalSettings, target.PlanSettings);
        Assert.Same(originalSchemes, target.PlanColorSchemes);
        Assert.NotSame(source.PlanSettings, target.PlanSettings);
        Assert.NotSame(source.PlanColorSchemes, target.PlanColorSchemes);
    }

    [Fact]
    public void Comparer_EmployeeGroup_UsesOrderedCompositeKey()
    {
        EmployeeGroup first = new() { EmployeeId = 10, GroupId = 20 };
        EmployeeGroup same = new() { EmployeeId = 10, GroupId = 20 };
        EmployeeGroup reversed = new() { EmployeeId = 20, GroupId = 10 };

        Assert.True(EmployeeGroup.Comparer.Equals(first, same));
        Assert.Equal(
            EmployeeGroup.Comparer.GetHashCode(first),
            EmployeeGroup.Comparer.GetHashCode(same));
        Assert.False(EmployeeGroup.Comparer.Equals(first, reversed));
    }
}
