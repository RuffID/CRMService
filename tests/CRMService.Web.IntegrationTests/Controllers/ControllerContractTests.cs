using System.Net;
using System.Text.Json;
using CRMService.Web.IntegrationTests.Infrastructure;
using CRMService.Web.Service.BackgroundServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CRMService.Web.IntegrationTests.Controllers;

public class ControllerContractTests
{
    [Theory]
    [InlineData(true, StatusCodes.Status202Accepted, "Обновление запущено.")]
    [InlineData(false, StatusCodes.Status409Conflict, "Обновление уже выполняется.")]
    public void ToBackgroundUpdateResponse_StartState_ReturnsExpectedContract(
        bool started,
        int expectedStatus,
        string expectedMessage)
    {
        ProbeController controller = new();

        ObjectResult result = Assert.IsAssignableFrom<ObjectResult>(controller.ToBackgroundUpdateResponse(started));
        string payload = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using JsonDocument json = JsonDocument.Parse(payload);

        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.Equal(expectedMessage, json.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task IssueUpdate_InvalidPeriod_ReturnsBadRequestBeforeBackgroundOperation()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.USER_HEADER, "test-user");
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.ROLE_HEADER, "admin");

        using HttpRequestMessage request = new(
            HttpMethod.Put,
            "/api/issue/update_from_cloud_api?dateFrom=2026-08-28T00:00:00Z&dateTo=2026-08-27T00:00:00Z");
        using HttpResponseMessage response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "Start date is later than end date.",
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, factory.HttpApiClientControl.CallCount);
        Assert.Equal(0, factory.NotificationService.SendCount);
    }

    private class ProbeController : ControllerBase;
}
