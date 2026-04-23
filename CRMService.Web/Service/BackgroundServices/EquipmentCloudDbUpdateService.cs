using CRMService.Application.Service.OkdeskEntity;
using CRMService.Contracts.Models.Responses.Results;

namespace CRMService.Web.Service.BackgroundServices
{
    public class EquipmentCloudDbUpdateService(IServiceScopeFactory scopeFactory, ILogger<EquipmentCloudDbUpdateService> logger)
    {
        private readonly Lock syncRoot = new();
        private EquipmentCloudDbUpdateStateDto? state;

        public EquipmentCloudDbUpdateStateDto GetState()
        {
            lock (syncRoot)
            {
                return state ?? EquipmentCloudDbUpdateStateDto.NotRunning();
            }
        }

        public ServiceResult<bool> TryStart(string userName)
        {
            DateTime startedAtUtc = DateTime.UtcNow;

            lock (syncRoot)
            {
                if (state != null)
                {
                    return ServiceResult<bool>.Fail(
                        StatusCodes.Status409Conflict,
                        BuildAlreadyRunningMessage(state));
                }

                state = EquipmentCloudDbUpdateStateDto.Running(startedAtUtc, userName);
            }

            _ = Task.Run(RunUpdateAsync);

            return ServiceResult<bool>.Ok(true);
        }

        private async Task RunUpdateAsync()
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                EquipmentService equipmentService = scope.ServiceProvider.GetRequiredService<EquipmentService>();
                await equipmentService.UpdateEquipmentsFromCloudDbAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Method:{MethodName}] Equipment cloud DB update failed.", nameof(RunUpdateAsync));
            }
            finally
            {
                lock (syncRoot)
                {
                    state = null;
                }
            }
        }

        private static string BuildAlreadyRunningMessage(EquipmentCloudDbUpdateStateDto runningState)
        {
            return $"Обновление уже запущено| Время запуска:{runningState.StartedAtUtc:O}| Кем запущено: {runningState.StartedByUserName}";
        }
    }

    public class EquipmentCloudDbUpdateStateDto
    {
        public bool IsRunning { get; init; }

        public DateTime? StartedAtUtc { get; init; }

        public string StartedByUserName { get; init; } = string.Empty;

        public static EquipmentCloudDbUpdateStateDto NotRunning()
        {
            return new EquipmentCloudDbUpdateStateDto
            {
                IsRunning = false
            };
        }

        public static EquipmentCloudDbUpdateStateDto Running(DateTime startedAtUtc, string startedByUserName)
        {
            return new EquipmentCloudDbUpdateStateDto
            {
                IsRunning = true,
                StartedAtUtc = DateTime.SpecifyKind(startedAtUtc, DateTimeKind.Utc),
                StartedByUserName = startedByUserName
            };
        }
    }
}
