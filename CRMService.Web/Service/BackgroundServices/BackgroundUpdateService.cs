using System.Collections.Concurrent;

namespace CRMService.Web.Service.BackgroundServices
{
    public class BackgroundUpdateService(IServiceScopeFactory scopeFactory, ILogger<BackgroundUpdateService> logger)
    {
        private readonly ConcurrentDictionary<string, byte> runningOperations = new();

        public bool TryStart(string operationKey, Func<IServiceProvider, CancellationToken, Task> operation)
        {
            if (string.IsNullOrWhiteSpace(operationKey))
                throw new ArgumentException("Operation key must be specified.", nameof(operationKey));

            ArgumentNullException.ThrowIfNull(operation);

            if (!runningOperations.TryAdd(operationKey, 0))
                return false;

            _ = Task.Run(() => RunAsync(operationKey, operation));

            return true;
        }

        private async Task RunAsync(string operationKey, Func<IServiceProvider, CancellationToken, Task> operation)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                await operation(scope.ServiceProvider, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Method:{MethodName}][Operation:{OperationKey}] Background update failed.", nameof(RunAsync), operationKey);
            }
            finally
            {
                runningOperations.TryRemove(operationKey, out _);
            }
        }
    }
}
