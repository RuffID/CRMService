using System.Net;
using System.Text.Json;
using CRMService.Web.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CRMService.Web.IntegrationTests.Core;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task ExpectedApplicationFailure_ReturnsMappedStatusAndPayload()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/test-probe/expected-failure",
            TestContext.Current.CancellationToken);
        using JsonDocument json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Expected conflict.", json.RootElement.GetProperty("message").GetString());
    }

    [Theory]
    [InlineData("/api/test-probe/infrastructure-error")]
    [InlineData("/api/test-probe/unknown-error")]
    public async Task UnexpectedException_ReturnsSafeInternalServerError(string path)
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.GetAsync(path, TestContext.Current.CancellationToken);
        string payload = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using JsonDocument json = JsonDocument.Parse(payload);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Internal server error", json.RootElement.GetProperty("error").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("traceId").GetString()));
        Assert.DoesNotContain("secret-value", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OperationCancelled_ReturnsClientClosedRequestWithoutPayloadLeak()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/test-probe/cancelled",
            TestContext.Current.CancellationToken);

        Assert.Equal(499, (int)response.StatusCode);
        Assert.Equal(
            string.Empty,
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static HttpClient CreateClient(CrmWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}
