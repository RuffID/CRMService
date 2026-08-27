using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class MaintenanceEntityService(IOptions<ApiEndpointOptions> endpoint,
        IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IEquipmentUnitOfWork unitOfWork, IOkdeskEquipmentSource okdeskUnitOfWork, EntitySyncService sync, ILogger<MaintenanceEntityService> logger)
    {
        public Task<MaintenanceEntity?> GetMaintenanceEntityAsync(int id, CancellationToken ct = default) =>
            unitOfWork.MaintenanceEntity.GetItemByIdReadOnlyAsync(id, ct);

        public Task<List<MaintenanceEntity>> GetMaintenanceEntitiesAsync(CancellationToken ct = default) =>
            unitOfWork.MaintenanceEntity.GetItemsReadOnlyAsync(ct);

        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<LookupOptionDto>>> GetMaintenanceEntityLookupAsync(EquipmentLookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);
            List<int>? companyIds = NormalizeIds(requestModel.CompanyIds);

            List<MaintenanceEntity> maintenanceEntities = await unitOfWork.MaintenanceEntity.SearchReadOnlyAsync(normalizedSearch, companyIds, ct);

            IEnumerable<MaintenanceEntity> orderedMaintenanceEntities = normalizedSearch == null
                ? maintenanceEntities.OrderBy(maintenanceEntity => maintenanceEntity.Id)
                : maintenanceEntities
                    .OrderBy(maintenanceEntity => GetSearchRank(maintenanceEntity, normalizedSearch))
                    .ThenBy(maintenanceEntity => maintenanceEntity.Name)
                    .ThenBy(maintenanceEntity => maintenanceEntity.Id);

            List<LookupOptionDto> items = orderedMaintenanceEntities
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(maintenanceEntity => new LookupOptionDto
                {
                    Id = maintenanceEntity.Id,
                    Text = FormatMaintenanceEntityText(maintenanceEntity)
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        public async Task<MaintenanceEntity?> GetMaintenanceEntityFromCloudApi(int maintenanceEntityId, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/{maintenanceEntityId}?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetItemAsync<MaintenanceEntity>(link, ct);
        }

        private async IAsyncEnumerable<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/list?api_token={okdeskSettings.Value.OkdeskApiToken}";
            await foreach (List<MaintenanceEntity> me in request.GetAllItemsAsync<MaintenanceEntity>(link, startIndex: 0, limit, ct: ct))
                yield return me;
        }

        private async Task<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudDb(CancellationToken ct)
        {
            List<MaintenanceEntity> maintenanceEntities = await okdeskUnitOfWork.MaintenanceEntity.GetAllWithCompanyReadOnlyAsync(ct);

            foreach (MaintenanceEntity item in maintenanceEntities)
                item.CompanyId = item.Company?.Id;

            return maintenanceEntities.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateMaintenanceEntityFromCloudApi(int maintenanceEntityId, CancellationToken ct)
        {
            MaintenanceEntity? newMaintenanceEntity = await GetMaintenanceEntityFromCloudApi(maintenanceEntityId, ct);

            if (newMaintenanceEntity == null)
                return;

            await sync.RunExclusive(newMaintenanceEntity, async () =>
            {
                await CreateOrUpdate(newMaintenanceEntity, ct);
            }, ct);
        }

        public async Task UpdateMaintenanceEntitiesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update maintenance entities from API.", nameof(UpdateMaintenanceEntitiesFromCloudApi));

            await foreach (List<MaintenanceEntity> me in GetMaintenanceEntitiesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (MaintenanceEntity newMaintenanceEntity in me)
                {
                    await sync.RunExclusive(newMaintenanceEntity, async () =>
                    {
                        await CreateOrUpdate(newMaintenanceEntity, ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update maintenance entities completed.", nameof(UpdateMaintenanceEntitiesFromCloudApi));
        }

        public async Task UpdateMaintenanceEntitiesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting updating maintenance entities.", nameof(UpdateMaintenanceEntitiesFromCloudDb));

            List<MaintenanceEntity> maintenanceEntities = await GetMaintenanceEntitiesFromCloudDb(ct);

            if (maintenanceEntities.Count == 0)
                return;

            foreach (MaintenanceEntity newMaintenanceEntity in maintenanceEntities)
            {
                await sync.RunExclusive(newMaintenanceEntity, async () =>
                {
                    await CreateOrUpdate(newMaintenanceEntity, ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudDb));
        }

        public async Task CreateOrUpdate(MaintenanceEntity maintenanceEntity, CancellationToken ct)
        {
            await CheckMaintenanceEntity(maintenanceEntity, ct);

            MaintenanceEntity? existingMaintenance = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(maintenanceEntity.Id, ct);
            if (existingMaintenance == null)
                unitOfWork.MaintenanceEntity.Create(maintenanceEntity);
            else
                existingMaintenance.CopyData(maintenanceEntity);

            await unitOfWork.SaveChangesAsync(ct);
        }

        private async Task CheckMaintenanceEntity(MaintenanceEntity maintenanceEntity, CancellationToken ct)
        {
            if (maintenanceEntity.Company != null)
            {
                Company? company = await unitOfWork.Company.GetItemByIdReadOnlyAsync(maintenanceEntity.Company.Id, ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for maintenanceEntity with id: {maintenanceEntityId}.",
                        nameof(CheckMaintenanceEntity), maintenanceEntity.Company.Id, maintenanceEntity.Id);
                    maintenanceEntity.CompanyId = null;
                }
                else
                {
                    maintenanceEntity.CompanyId = company.Id;
                }
            }
            else if (maintenanceEntity.CompanyId.HasValue)
            {
                Company? company = await unitOfWork.Company.GetItemByIdReadOnlyAsync(maintenanceEntity.CompanyId.Value, ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for maintenanceEntity with id: {maintenanceEntityId}.",
                        nameof(CheckMaintenanceEntity), maintenanceEntity.CompanyId, maintenanceEntity.Id);
                    maintenanceEntity.CompanyId = null;
                }
                else
                {
                    maintenanceEntity.CompanyId = company.Id;
                }
            }

            maintenanceEntity.Company = null;
        }

        private static ServiceResult ValidateLookupRequest(LookupListRequest request)
        {
            if (request.Offset < 0)
                return ServiceResult.Fail(400, "Смещение не может быть отрицательным.");

            if (request.Limit == 0)
                request.Limit = DEFAULT_LOOKUP_LIMIT;

            if (request.Limit <= 0 || request.Limit > 100)
                return ServiceResult.Fail(400, "Лимит должен быть в диапазоне от 1 до 100.");

            return ServiceResult.Ok();
        }

        private static string? NormalizeSearch(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim();
            int nonWhitespaceCount = normalized.Count(character => !char.IsWhiteSpace(character));
            return nonWhitespaceCount >= 2 ? normalized : null;
        }

        private static List<int>? NormalizeIds(List<int>? ids)
        {
            if (ids == null || ids.Count == 0)
                return null;

            List<int> values = ids
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            return values.Count == 0 ? null : values;
        }

        private static string FormatMaintenanceEntityText(MaintenanceEntity maintenanceEntity)
        {
            if (maintenanceEntity.Company == null || string.IsNullOrWhiteSpace(maintenanceEntity.Company.Name))
                return maintenanceEntity.Name;

            return $"{maintenanceEntity.Name} ({maintenanceEntity.Company.Name})";
        }

        private static int GetSearchRank(MaintenanceEntity maintenanceEntity, string search)
        {
            if (maintenanceEntity.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (!string.IsNullOrWhiteSpace(maintenanceEntity.Company?.Name) && maintenanceEntity.Company.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (!string.IsNullOrWhiteSpace(maintenanceEntity.Address) && maintenanceEntity.Address.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 2;

            if (maintenanceEntity.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 3;

            if (!string.IsNullOrWhiteSpace(maintenanceEntity.Company?.Name) && maintenanceEntity.Company.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 4;

            if (!string.IsNullOrWhiteSpace(maintenanceEntity.Address) && maintenanceEntity.Address.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 5;

            return 6;
        }
    }
}
