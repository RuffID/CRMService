using CRMService.Application.Service.OkdeskEntity.Resolvers;
using CRMService.Application.Service.Sync;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CRMService.Application.Tests.Service.OkdeskEntity.Resolvers;

public class ReferenceResolveHelperTests
{
    [Fact]
    public async Task ResolveAsync_LocalHit_ReturnsIdWithoutRefresh()
    {
        ReferenceResolveHelper helper = new(new EntitySyncService());
        int refreshCalls = 0;

        int? result = await Resolve(
            helper,
            _ => Task.FromResult<int?>(17),
            _ =>
            {
                refreshCalls++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(17, result);
        Assert.Equal(0, refreshCalls);
    }

    [Fact]
    public async Task ResolveAsync_MissingThenFoundAfterRefresh_RefreshesOnceAndReturnsId()
    {
        ReferenceResolveHelper helper = new(new EntitySyncService());
        int lookupCalls = 0;
        int refreshCalls = 0;

        int? result = await Resolve(
            helper,
            _ => Task.FromResult<int?>(++lookupCalls >= 3 ? 23 : null),
            _ =>
            {
                refreshCalls++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(23, result);
        Assert.Equal(3, lookupCalls);
        Assert.Equal(1, refreshCalls);
    }

    [Fact]
    public async Task ResolveAsync_MissingAfterRefresh_ReturnsNull()
    {
        ReferenceResolveHelper helper = new(new EntitySyncService());
        int refreshCalls = 0;

        int? result = await Resolve(
            helper,
            _ => Task.FromResult<int?>(null),
            _ =>
            {
                refreshCalls++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(1, refreshCalls);
    }

    [Fact]
    public async Task ResolveAsync_RefreshThrows_PropagatesException()
    {
        ReferenceResolveHelper helper = new(new EntitySyncService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => Resolve(
            helper,
            _ => Task.FromResult<int?>(null),
            _ => throw new InvalidOperationException("refresh failed"),
            CancellationToken.None));
    }

    [Fact]
    public async Task ResolveAsync_LookupCancels_PropagatesCancellation()
    {
        ReferenceResolveHelper helper = new(new EntitySyncService());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Resolve(
            helper,
            _ => Task.FromCanceled<int?>(cancellation.Token),
            _ => Task.CompletedTask,
            cancellation.Token));
    }

    private static Task<int?> Resolve(
        ReferenceResolveHelper helper,
        Func<CancellationToken, Task<int?>> lookup,
        Func<CancellationToken, Task> refresh,
        CancellationToken ct)
    {
        return helper.ResolveAsync(
            5,
            lookup,
            refresh,
            id => $"entity:{id}",
            id => $"Entity {id} was not found.",
            id => $"Entity {id} was not found after refresh.",
            Substitute.For<ILogger>(),
            "Resolve",
            ct);
    }
}
