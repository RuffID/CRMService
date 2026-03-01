namespace CRMService.Application.Service.Sync
{
    public class EntitySyncService
    {
        private readonly SemaphoreSlim globalLock = new(1, 1);

        public async Task RunExclusive(Func<Task> action)
        {
            Task waitTask = globalLock.WaitAsync();

            await waitTask;

            try
            {
                await action();
            }
            finally
            {
                globalLock.Release();
            }
        }
    }
}