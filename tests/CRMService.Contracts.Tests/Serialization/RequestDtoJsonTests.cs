using System.Text.Json;
using CRMService.Contracts.Models.Request;
using Xunit;

namespace CRMService.Contracts.Tests.Serialization;

public class RequestDtoJsonTests
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void CreateUserRequest_SerializeThenDeserialize_PreservesRazorPagePayload()
    {
        Guid firstRoleId = Guid.NewGuid();
        Guid secondRoleId = Guid.NewGuid();
        CreateUserRequest expected = new()
        {
            Name = "User name",
            Login = "user.login",
            Password = "secret",
            EmployeeId = 42,
            RoleIds = new List<Guid> { firstRoleId, secondRoleId }
        };

        string json = JsonSerializer.Serialize(expected, WebJsonOptions);
        CreateUserRequest? actual = JsonSerializer.Deserialize<CreateUserRequest>(json, WebJsonOptions);

        Assert.NotNull(actual);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Login, actual.Login);
        Assert.Equal(expected.Password, actual.Password);
        Assert.Equal(expected.EmployeeId, actual.EmployeeId);
        Assert.Equal(expected.RoleIds, actual.RoleIds);
        Assert.Contains("\"employeeId\"", json);
        Assert.Contains("\"roleIds\"", json);
    }

    [Fact]
    public void CreateUserRequest_OptionalFieldsAreAbsent_DeserializesThemAsNull()
    {
        const string json = "{\"name\":\"User\",\"login\":\"login\",\"password\":\"secret\"}";

        CreateUserRequest? actual = JsonSerializer.Deserialize<CreateUserRequest>(json, WebJsonOptions);

        Assert.NotNull(actual);
        Assert.Null(actual.EmployeeId);
        Assert.Null(actual.RoleIds);
    }

    [Fact]
    public void IssueListRequest_SerializeThenDeserialize_PreservesFiltersAndPaging()
    {
        IssueListRequest expected = new()
        {
            NumberFrom = 100,
            NumberTo = 200,
            Search = "printer",
            AssigneeIds = new List<int> { 1, 2 },
            StatusIds = new List<int> { 3 },
            RegistrationDateFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ResolutionDateTo = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc),
            Page = 3,
            PageSize = 50
        };

        string json = JsonSerializer.Serialize(expected, WebJsonOptions);
        IssueListRequest? actual = JsonSerializer.Deserialize<IssueListRequest>(json, WebJsonOptions);

        Assert.NotNull(actual);
        Assert.Equal(expected.NumberFrom, actual.NumberFrom);
        Assert.Equal(expected.NumberTo, actual.NumberTo);
        Assert.Equal(expected.Search, actual.Search);
        Assert.Equal(expected.AssigneeIds, actual.AssigneeIds);
        Assert.Equal(expected.StatusIds, actual.StatusIds);
        Assert.Equal(expected.RegistrationDateFrom, actual.RegistrationDateFrom);
        Assert.Equal(expected.ResolutionDateTo, actual.ResolutionDateTo);
        Assert.Equal(expected.Page, actual.Page);
        Assert.Equal(expected.PageSize, actual.PageSize);
    }

    [Fact]
    public void EquipmentListRequest_OptionalFiltersAreAbsent_UsesNullsAndPagingDefaults()
    {
        EquipmentListRequest? actual =
            JsonSerializer.Deserialize<EquipmentListRequest>("{}", WebJsonOptions);

        Assert.NotNull(actual);
        Assert.Null(actual.EquipmentId);
        Assert.Null(actual.TypeIds);
        Assert.Null(actual.ManufacturerIds);
        Assert.Null(actual.ModelIds);
        Assert.Null(actual.CompanyIds);
        Assert.Null(actual.MaintenanceEntityIds);
        Assert.Equal(1, actual.Page);
        Assert.Equal(20, actual.PageSize);
    }
}
