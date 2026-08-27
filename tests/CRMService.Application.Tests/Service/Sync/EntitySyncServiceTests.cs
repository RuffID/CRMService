using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Xunit;

namespace CRMService.Application.Tests.Service.Sync;

public class EntitySyncServiceTests
{
    [Fact]
    public async Task RunExclusive_SameTypeAndId_DoesNotOverlapActions()
    {
        EntitySyncService service = new();
        Issue first = new() { Id = 10 };
        Issue second = new() { Id = 10 };
        TaskCompletionSource firstEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirst = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool secondEntered = false;

        Task firstTask = service.RunExclusive(first, async () =>
        {
            firstEntered.SetResult();
            await releaseFirst.Task;
        }, TestContext.Current.CancellationToken);
        await firstEntered.Task;

        Task secondTask = service.RunExclusive(second, () =>
        {
            secondEntered = true;
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken);

        await Task.Yield();
        Assert.False(secondEntered);

        releaseFirst.SetResult();
        await Task.WhenAll(firstTask, secondTask);
        Assert.True(secondEntered);
    }

    [Fact]
    public async Task RunExclusive_DifferentIds_AllowsParallelActions()
    {
        EntitySyncService service = new();
        TaskCompletionSource firstEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task firstTask = service.RunExclusive(new Issue { Id = 1 }, async () =>
        {
            firstEntered.SetResult();
            await release.Task;
        }, TestContext.Current.CancellationToken);
        Task secondTask = service.RunExclusive(new Issue { Id = 2 }, async () =>
        {
            secondEntered.SetResult();
            await release.Task;
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(firstEntered.Task, secondEntered.Task).WaitAsync(
            TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken);
        release.SetResult();
        await Task.WhenAll(firstTask, secondTask);
    }

    [Fact]
    public async Task RunExclusive_FirstActionThrows_ReleasesLockForNextAction()
    {
        EntitySyncService service = new();
        Issue entity = new() { Id = 7 };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RunExclusive(
                entity,
                () => throw new InvalidOperationException("failure"),
                TestContext.Current.CancellationToken));

        bool executed = false;
        await service.RunExclusive(entity, () =>
        {
            executed = true;
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken);

        Assert.True(executed);
    }

    [Fact]
    public async Task RunExclusive_WaitingIsCancelled_DoesNotRunCancelledAction()
    {
        EntitySyncService service = new();
        Issue entity = new() { Id = 8 };
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task holder = service.RunExclusive(entity, async () =>
        {
            entered.SetResult();
            await release.Task;
        }, TestContext.Current.CancellationToken);
        await entered.Task;
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        bool executed = false;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.RunExclusive(entity, () =>
            {
                executed = true;
                return Task.CompletedTask;
            }, cancellation.Token));

        Assert.False(executed);
        release.SetResult();
        await holder;
    }

    [Fact]
    public async Task RunExclusive_NullArguments_ThrowArgumentNullException()
    {
        EntitySyncService service = new();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.RunExclusive<int>(
                null!,
                () => Task.CompletedTask,
                TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.RunExclusive(
                new Issue { Id = 1 },
                null!,
                TestContext.Current.CancellationToken));
    }
}
