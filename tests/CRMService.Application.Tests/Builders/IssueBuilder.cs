using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Tests.Builders;

public class IssueBuilder
{
    private int id = 1;
    private string title = "Test issue";

    public IssueBuilder WithId(int value)
    {
        id = value;
        return this;
    }

    public Issue Build()
    {
        return new Issue
        {
            Id = id,
            Title = title,
            CreatedAt = new DateTime(2026, 1, 1)
        };
    }
}
