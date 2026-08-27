using System.Net;
using System.Text.Json;
using CRMService.Application.Models.ConfigClass;
using CRMService.Infrastructure.Service.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Service.Requests;

public class TelegramNotificationTests
{
    [Fact]
    public async Task SendMessage_ValidInput_SendsExpectedRequest()
    {
        CaptureHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient client = new(handler);
        TelegramNotification service = CreateService(client);

        await service.SendMessage(42, "test message", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://telegram.invalid/send?chatId=42", handler.Url);
        Assert.Equal("test-token", handler.Authorization);
        using JsonDocument json = JsonDocument.Parse(handler.Body!);
        Assert.Equal("test message", json.RootElement.GetProperty("message").GetString());
    }

    [Theory]
    [InlineData(null, "message")]
    [InlineData(0L, "message")]
    [InlineData(42L, "")]
    [InlineData(42L, "   ")]
    public async Task SendMessage_InvalidInput_DoesNotSend(long? chatId, string content)
    {
        CaptureHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient client = new(handler);

        await CreateService(client).SendMessage(chatId, content, TestContext.Current.CancellationToken);

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task SendMessage_UnsuccessfulResponse_IsHandledWithoutRetry()
    {
        CaptureHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadGateway));
        using HttpClient client = new(handler);

        await CreateService(client).SendMessage(42, "message", TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SendMessage_Cancellation_IsHandledAndTokenReachesHandler()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        CaptureHandler handler = new(_ => throw new OperationCanceledException(cancellation.Token));
        using HttpClient client = new(handler);

        await CreateService(client).SendMessage(42, "message", cancellation.Token);

        Assert.True(handler.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public async Task SendMessage_MissingToken_ThrowsInvalidOperationException()
    {
        CaptureHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient client = new(handler);
        TelegramNotification service = CreateService(client, token: "");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendMessage(42, "message", TestContext.Current.CancellationToken));
    }

    private static TelegramNotification CreateService(HttpClient client, string token = "test-token") =>
        new(
            client,
            Options.Create(new ApiEndpointOptions { TelegramBotUrl = "https://telegram.invalid/send" }),
            Options.Create(new TelegramBotOptions { Token = token }),
            NullLogger<TelegramNotification>.Instance);

    private class CaptureHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Url { get; private set; }
        public string? Authorization { get; private set; }
        public string? Body { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            Method = request.Method;
            Url = request.RequestUri?.ToString();
            Authorization = request.Headers.GetValues("Authorization").Single();
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            CancellationToken = cancellationToken;
            return responseFactory(request);
        }
    }
}
