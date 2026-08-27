using System.Net;
using System.Text;
using CRMService.Web.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CRMService.Web.IntegrationTests.Controllers;

public class WebhookControllerTests
{
    [Fact]
    public async Task Webhook_AllowedForwardedAddress_SelectsFirstMatchingHandler()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory, "203.0.113.10");
        using StringContent content = JsonContent("{\"event\":{\"event_type\":\"issue_created\"}}");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/webhook",
            content,
            TestContext.Current.CancellationToken);
        await factory.FirstWebhookHandler.WaitForCallsAsync(1);
        await factory.MatchingWebhookHandler.WaitForCallsAsync(1);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, factory.FirstWebhookHandler.CallCount);
        Assert.Equal(1, factory.MatchingWebhookHandler.CallCount);
        Assert.Equal(0, factory.LastWebhookHandler.CallCount);
        Assert.All(factory.MatchingWebhookHandler.Tokens, token => Assert.False(token.CanBeCanceled));
        Assert.Equal(0, factory.HttpApiClientControl.CallCount);
        Assert.Equal(0, factory.NotificationService.SendCount);
    }

    [Fact]
    public async Task Webhook_ForbiddenForwardedAddress_ReturnsForbiddenWithoutDispatch()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory, "192.0.2.20");
        using StringContent content = JsonContent("{\"event\":{\"event_type\":\"issue_created\"}}");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/webhook",
            content,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, factory.FirstWebhookHandler.CallCount);
    }

    [Theory]
    [InlineData("not-json", "Invalid JSON")]
    [InlineData("{}", "Empty event or action object.")]
    public async Task Webhook_InvalidOrEmptyPayload_ReturnsBadRequest(string payload, string expectedMessage)
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory, "203.0.113.10");
        using StringContent content = JsonContent(payload);

        using HttpResponseMessage response = await client.PostAsync(
            "/api/webhook",
            content,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            expectedMessage,
            (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Trim('"'));
        Assert.Equal(0, factory.FirstWebhookHandler.CallCount);
    }

    [Fact]
    public async Task Webhook_UnknownEvent_InvokesAllHandlersAndStillAcknowledgesReceipt()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory, "198.51.100.42");
        using StringContent content = JsonContent("{\"event\":{\"event_type\":\"unknown_event\"}}");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/webhook",
            content,
            TestContext.Current.CancellationToken);
        await factory.LastWebhookHandler.WaitForCallsAsync(1);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, factory.FirstWebhookHandler.CallCount);
        Assert.Equal(1, factory.MatchingWebhookHandler.CallCount);
        Assert.Equal(1, factory.LastWebhookHandler.CallCount);
        Assert.Equal(0, factory.HttpApiClientControl.CallCount);
        Assert.Equal(0, factory.NotificationService.SendCount);
    }

    private static HttpClient CreateClient(CrmWebApplicationFactory factory, string forwardedAddress)
    {
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Forwarded-For", forwardedAddress);
        return client;
    }

    private static StringContent JsonContent(string payload) =>
        new(payload, Encoding.UTF8, "application/json");
}
