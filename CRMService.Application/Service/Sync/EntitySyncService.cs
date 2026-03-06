using EFCoreLibrary.Abstractions.Entity;
using System.Collections.Concurrent;

namespace CRMService.Application.Service.Sync
{
    public class EntitySyncService
    {
        private readonly ConcurrentDictionary<EntitySyncKey, LockEntry> locks = new();

        public async Task RunExclusive<TId>(IEntity<TId> entity, Func<Task> action, CancellationToken ct = default)
            where TId : notnull, IEquatable<TId>
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(action);

            EntitySyncKey key = new(entity.GetType(), entity.Id);
            LockEntry entry = locks.AddOrUpdate(key, _ => new LockEntry(), (_, existing) => existing);

            Interlocked.Increment(ref entry.UsersCount);
            await entry.Semaphore.WaitAsync(ct);

            try
            {
                await action();
            }
            finally
            {
                entry.Semaphore.Release();

                if (Interlocked.Decrement(ref entry.UsersCount) == 0 
                    && locks.TryRemove(new KeyValuePair<EntitySyncKey, LockEntry>(key, entry)))
                {
                    entry.Semaphore.Dispose();
                }
            }
        }

        private readonly record struct EntitySyncKey(Type EntityType, object Id);

        private sealed class LockEntry
        {
            public SemaphoreSlim Semaphore { get; } = new(1, 1);
            public int UsersCount;
        }
    }
}