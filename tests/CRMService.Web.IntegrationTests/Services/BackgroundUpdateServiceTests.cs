using CRMService.Web.Service.BackgroundServices;
using Microsoft.Extensions.Logging.Abstractions;

namespace CRMService.Web.IntegrationTests.Services;

public class BackgroundUpdateServiceTests
{
    [Fact]
    public async Task TryStart_DuplicateOperation_ReturnsConflictAndReleasesKeyAfterCompletion()
    {
        ServiceCollection registrations = new();
        await using ServiceProvider provider = registrations.BuildServiceProvider();
        BackgroundUpdateService service = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<BackgroundUpdateService>.Instance);
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken receivedToken = new(canceled: true);

        bool firstStart = service.TryStart("operation", async (_, token) =>
        {
            receivedToken = token;
            started.SetResult();
            await release.Task;
        });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        bool duplicateStart = service.TryStart("operation", (_, _) => Task.CompletedTask);
        release.SetResult();

        Assert.True(firstStart);
        Assert.False(duplicateStart);
        Assert.False(receivedToken.CanBeCanceled);

        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!service.TryStart("operation", (_, _) => Task.CompletedTask))
        {
            if (DateTime.UtcNow >= deadline)
                throw new TimeoutException("Background operation key was not released.");

            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }
}
