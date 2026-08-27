using System.Text.Json;
using System.Text.Json.Serialization;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Contracts.Models.Responses;
using Xunit;

namespace CRMService.Contracts.Tests.Serialization;

public class ResponseDtoJsonTests
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void IssueTypeResponse_ExternalJsonName_RoundTripsAvailableForClient()
    {
        const string json =
            "{\"id\":7,\"name\":\"Repair\",\"code\":\"repair\",\"available_for_client\":true}";

        IssueTypeResponse? actual = JsonSerializer.Deserialize<IssueTypeResponse>(json, WebJsonOptions);
        string serialized = JsonSerializer.Serialize(actual, WebJsonOptions);

        Assert.NotNull(actual);
        Assert.Equal(7, actual.Id);
        Assert.Equal("Repair", actual.Name);
        Assert.Equal("repair", actual.Code);
        Assert.True(actual.AvailableForClient);
        Assert.Null(actual.Children);
        Assert.Contains("\"available_for_client\":true", serialized);
        Assert.DoesNotContain("availableForClient", serialized);
        Assert.DoesNotContain("\"children\"", serialized);
    }

    [Fact]
    public void IssueDetailsDto_SerializeThenDeserialize_PreservesResponseAndNullableDates()
    {
        IssueDetailsDto expected = new()
        {
            Id = 91,
            Title = "Printer repair",
            CompanyName = "Company",
            CompanyCategoryColor = "#112233",
            ServiceObjectName = "Office",
            AssigneeName = "Assignee",
            AuthorName = "Author",
            TypeName = "Repair",
            StatusName = "In progress",
            StatusColor = "#445566",
            PriorityName = "High",
            PriorityColor = "#778899",
            CreatedAt = new DateTime(2026, 6, 1, 12, 30, 0, DateTimeKind.Utc),
            CompletedAt = null,
            DeadlineAt = new DateTime(2026, 6, 2, 12, 30, 0, DateTimeKind.Utc),
            DelayTo = null
        };

        string json = JsonSerializer.Serialize(expected, WebJsonOptions);
        IssueDetailsDto? actual = JsonSerializer.Deserialize<IssueDetailsDto>(json, WebJsonOptions);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.CompanyName, actual.CompanyName);
        Assert.Equal(expected.CompanyCategoryColor, actual.CompanyCategoryColor);
        Assert.Equal(expected.ServiceObjectName, actual.ServiceObjectName);
        Assert.Equal(expected.AssigneeName, actual.AssigneeName);
        Assert.Equal(expected.AuthorName, actual.AuthorName);
        Assert.Equal(expected.TypeName, actual.TypeName);
        Assert.Equal(expected.StatusName, actual.StatusName);
        Assert.Equal(expected.StatusColor, actual.StatusColor);
        Assert.Equal(expected.PriorityName, actual.PriorityName);
        Assert.Equal(expected.PriorityColor, actual.PriorityColor);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Null(actual.CompletedAt);
        Assert.Equal(expected.DeadlineAt, actual.DeadlineAt);
        Assert.Null(actual.DelayTo);
        Assert.Contains("\"createdAt\"", json);
        Assert.Contains("\"deadlineAt\"", json);
        Assert.DoesNotContain("\"completedAt\"", json);
        Assert.DoesNotContain("\"delayTo\"", json);
    }
}
