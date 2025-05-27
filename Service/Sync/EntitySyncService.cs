namespace CRMService.Service.Sync
{
    public class EntitySyncService(ILoggerFactory logger)
    {
        private readonly SemaphoreSlim globalLock = new(1, 1);
        private readonly ILogger<EntitySyncService> _logger = logger.CreateLogger<EntitySyncService>();

        public async Task RunExclusive(Func<Task> action, string context = "Global")
        {
            Task waitTask = globalLock.WaitAsync();

            if (!waitTask.IsCompleted)
            {
                _logger.LogInformation("[Method:{MethodName}] Global lock requested by {Context} — waiting...", nameof(RunExclusive), context);
            }

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
