using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Models.WebHook;
using CRMService.Web.Core.Startup;
using Microsoft.AspNetCore.Hosting;

namespace CRMService.Web.IntegrationTests.Infrastructure;

public class RecordingStartupInitializer : IStartupInitializer
{
    private int initializeCount;

    public int InitializeCount => Volatile.Read(ref initializeCount);

    public Task InitializeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Interlocked.Increment(ref initializeCount);
        return Task.CompletedTask;
    }
}

public class ControlledNotificationService : INotificationService
{
    private int sendCount;

    public int SendCount => Volatile.Read(ref sendCount);

    public Task SendMessage(long? chatId, string content, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Interlocked.Increment(ref sendCount);
        return Task.CompletedTask;
    }
}

public class ControlledWebhookHandler(Func<RootEventWebHook, bool> shouldHandle) : IWebhookHandler
{
    private readonly ConcurrentQueue<CancellationToken> tokens = new();
    private int callCount;

    public int CallCount => Volatile.Read(ref callCount);

    public IReadOnlyCollection<CancellationToken> Tokens => tokens.ToArray();

    public Task<bool> HandleWebhook(RootEventWebHook @event, CancellationToken ct)
    {
        tokens.Enqueue(ct);
        Interlocked.Increment(ref callCount);
        return Task.FromResult(shouldHandle(@event));
    }

    public async Task WaitForCallsAsync(int expectedCount)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (CallCount < expectedCount)
        {
            if (DateTime.UtcNow >= deadline)
                throw new TimeoutException($"Webhook handler received {CallCount} calls instead of {expectedCount}.");

            await Task.Delay(10);
        }
    }
}

public class TestProxyStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, continuation) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Loopback;
                await continuation();
            });

            next(app);
        };
    }
}

public class NoNetworkDispatchProxy : DispatchProxy
{
    private int callCount;

    public int CallCount => Volatile.Read(ref callCount);

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        Interlocked.Increment(ref callCount);
        throw new InvalidOperationException(
            $"External HTTP client call is forbidden in Web integration tests: {targetMethod?.Name}.");
    }
}
