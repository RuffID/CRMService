using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class EquipmentService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<EquipmentService> logger)
    {
        private async IAsyncEnumerable<List<Equipment>> GetEquipmentsFromCloudApi(long startIndex, long limit, long companyId = 0, long maintenanceEntityId = 0, [EnumeratorCancellation] CancellationToken ct = default)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.OkdeskApiToken}";

            if (companyId > 0)
                link += $"&company_ids[]={companyId}";
            if (maintenanceEntityId > 0)
                link += $"&maintenance_entity_ids[]={maintenanceEntityId}";

            await foreach (List<Equipment> equipments in request.GetAllItemsAsync<Equipment>(link: link, startIndex: startIndex, limit: limit, ct: ct))
                yield return equipments;
        }

        private async Task<List<Equipment>> GetEquipmentsFromCloudDb(int startId, int limit, CancellationToken ct)
        {
            List<Equipment> equipments = await okdeskUnitOfWork.Equipment.GetSyncItemsAsync(startId, limit, ct);

            return equipments.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateEquipmentFromCloudApi(long id, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.OkdeskApiToken}&page[from_id]={id}&page[direction]=forward&page[size]=1";

            List<Equipment>? equipments = await request.GetRangeOfItemsAsync<Equipment>(link, ct: ct);

            if (equipments == null || equipments.Count == 0)
                return;

            Equipment equipment = equipments.First();
            await sync.RunExclusive(equipment, async () =>
            {
                await CreateOrUpdate(equipment, ct);
            }, ct);
        }

        public async Task UpdateEquipmentsFromCloudApi(long companyId = 0, long maintenanceEntityId = 0, CancellationToken ct = default)
        {
            logger.LogInformation("[Method:{MethodName}] Starting updating equipments.", nameof(UpdateEquipmentsFromCloudApi));

            await foreach (List<Equipment> equipments in GetEquipmentsFromCloudApi(startIndex: 0, limit: LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, companyId: companyId, maintenanceEntityId: maintenanceEntityId, ct: ct))
            {
                if (equipments.Count == 0)
                    return;

                foreach (Equipment equipment in equipments)
                {
                    await sync.RunExclusive(equipment, async () =>
                    {
                        await CreateOrUpdate(equipment, ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Equipments update completed.", nameof(UpdateEquipmentsFromCloudApi));
        }

        public async Task UpdateEquipmentsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting ti update equipments.", nameof(UpdateEquipmentsFromCloudDb));

            int startId = 0;

            while (true)
            {
                List<Equipment> equipments = await GetEquipmentsFromCloudDb(startId, LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

                if (equipments.Count == 0)
                    break;

                EquipmentBatchContext batchContext = await CreateEquipmentBatchContext(equipments, ct);

                foreach (Equipment equipment in equipments)
                {
                    await sync.RunExclusive(equipment, async () =>
                    {
                        await CreateOrUpdateInternal(equipment, batchContext, ct);
                    }, ct);
                }

                startId = equipments.Last().Id;

                if (equipments.Count < LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}] Equipments update completed.", nameof(UpdateEquipmentsFromCloudDb));
        }

        public async Task CreateOrUpdate(Equipment equipment, CancellationToken ct)
        {
            EquipmentBatchContext batchContext = await CreateEquipmentBatchContext(new List<Equipment> { equipment }, ct);
            await CreateOrUpdateInternal(equipment, batchContext, ct);
        }

        private async Task CreateOrUpdateInternal(Equipment equipment, EquipmentBatchContext batchContext, CancellationToken ct)
        {
            CheckInformationOnEquipment(equipment, batchContext);

            Equipment? existingEquipment = await unitOfWork.Equipment.GetItemByIdAsync(equipment.Id, ct: ct);

            if (existingEquipment == null)
                unitOfWork.Equipment.Create(equipment);
            else
                existingEquipment.CopyData(equipment);

            List<EquipmentParameter> existingParameters = await unitOfWork.Parameter.GetItemsByPredicateAsync(p => p.EquipmentId == equipment.Id, ct: ct);
            Dictionary<int, EquipmentParameter> existingParametersByKindParameterId = existingParameters
                .Where(p => p.KindParameterId.HasValue)
                .ToDictionary(p => p.KindParameterId!.Value);

            foreach (EquipmentParameter parameter in equipment.Parameters)
            {
                int kindParameterId = parameter.KindParameterId
                    ?? throw new InvalidOperationException($"Equipment parameter kindParameterId is not set for equipment: {equipment.Id}.");

                if (!existingParametersByKindParameterId.TryGetValue(kindParameterId, out EquipmentParameter? existingParameter))
                    unitOfWork.Parameter.Create(parameter);
                else
                    existingParameter.CopyData(parameter);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        private void CheckInformationOnEquipment(Equipment equipment, EquipmentBatchContext batchContext)
        {
            if (equipment.Company != null)
                equipment.CompanyId = batchContext.CompanyIds.Contains(equipment.Company.Id) ? equipment.Company.Id : null;
            else if (equipment.CompanyId.HasValue && !batchContext.CompanyIds.Contains(equipment.CompanyId.Value))
                equipment.CompanyId = null;

            if (equipment.MaintenanceEntities != null)
                equipment.MaintenanceEntitiesId = batchContext.MaintenanceEntityIds.Contains(equipment.MaintenanceEntities.Id) ? equipment.MaintenanceEntities.Id : null;
            else if (equipment.MaintenanceEntitiesId.HasValue && !batchContext.MaintenanceEntityIds.Contains(equipment.MaintenanceEntitiesId.Value))
                equipment.MaintenanceEntitiesId = null;

            if (equipment.Manufacturer != null)
                equipment.ManufacturerId = batchContext.ManufacturerIdsByCode.GetValueOrDefault(equipment.Manufacturer.Code);

            if (equipment.Kind != null)
                equipment.KindId = batchContext.KindIdsByCode.GetValueOrDefault(equipment.Kind.Code);

            if (equipment.Model != null)
                equipment.ModelId = batchContext.ModelIdsByCode.GetValueOrDefault(equipment.Model.Code);

            equipment.Company = null;
            equipment.MaintenanceEntities = null;
            equipment.Manufacturer = null;
            equipment.Kind = null;
            equipment.Model = null;

            if (equipment.Parameters != null && equipment.Parameters.Count != 0)
            {
                equipment.Parameters.RemoveAll(p => string.IsNullOrEmpty(Convert.ToString(p.Value)));

                foreach (EquipmentParameter parameter in equipment.Parameters)
                {
                    parameter.EquipmentId = equipment.Id;
                    parameter.KindParameterId = batchContext.KindParameterIdsByCode.GetValueOrDefault(parameter.Code ?? string.Empty);

                    if (parameter.KindParameterId == 0)
                        throw new InvalidOperationException($"Equipment parameter: {parameter.Code} - not found.");
                }
            }
        }

        private async Task<EquipmentBatchContext> CreateEquipmentBatchContext(List<Equipment> equipments, CancellationToken ct)
        {
            HashSet<int> companyIds = equipments
                .Select(e => e.Company?.Id ?? e.CompanyId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            HashSet<int> maintenanceEntityIds = equipments
                .Select(e => e.MaintenanceEntities?.Id ?? e.MaintenanceEntitiesId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            HashSet<string> manufacturerCodes = equipments
                .Where(e => e.Manufacturer != null && !string.IsNullOrWhiteSpace(e.Manufacturer.Code))
                .Select(e => e.Manufacturer!.Code)
                .ToHashSet(StringComparer.Ordinal);
            HashSet<string> kindCodes = equipments
                .Where(e => e.Kind != null && !string.IsNullOrWhiteSpace(e.Kind.Code))
                .Select(e => e.Kind!.Code)
                .ToHashSet(StringComparer.Ordinal);
            HashSet<string> modelCodes = equipments
                .Where(e => e.Model != null && !string.IsNullOrWhiteSpace(e.Model.Code))
                .Select(e => e.Model!.Code)
                .ToHashSet(StringComparer.Ordinal);
            HashSet<string> parameterCodes = equipments
                .SelectMany(e => e.Parameters)
                .Where(p => !string.IsNullOrWhiteSpace(p.Code))
                .Select(p => p.Code!)
                .ToHashSet(StringComparer.Ordinal);

            List<Company> companies = companyIds.Count == 0
                ? new ()
                : await unitOfWork.Company.GetItemsByPredicateAsync(c => companyIds.Contains(c.Id), asNoTracking: true, ct: ct);
            List<MaintenanceEntity> maintenanceEntities = maintenanceEntityIds.Count == 0
                ? new ()
                : await unitOfWork.MaintenanceEntity.GetItemsByPredicateAsync(m => maintenanceEntityIds.Contains(m.Id), asNoTracking: true, ct: ct);
            List<Manufacturer> manufacturers = manufacturerCodes.Count == 0
                ? new ()
                : await unitOfWork.Manufacturer.GetItemsByPredicateAsync(m => manufacturerCodes.Contains(m.Code), asNoTracking: true, ct: ct);
            List<Kind> kinds = kindCodes.Count == 0
                ? new ()
                : await unitOfWork.Kind.GetItemsByPredicateAsync(k => kindCodes.Contains(k.Code), asNoTracking: true, ct: ct);
            List<Model> models = modelCodes.Count == 0
                ? new ()
                : await unitOfWork.Model.GetItemsByPredicateAsync(m => modelCodes.Contains(m.Code), asNoTracking: true, ct: ct);
            List<KindsParameter> kindParameters = parameterCodes.Count == 0
                ? new ()
                : await unitOfWork.KindParameter.GetItemsByPredicateAsync(kp => parameterCodes.Contains(kp.Code), asNoTracking: true, ct: ct);

            // Заполняет кэши связанных сущностей для пачки оборудования.
            return new EquipmentBatchContext(
                companies.Select(c => c.Id).ToHashSet(),
                maintenanceEntities.Select(m => m.Id).ToHashSet(),
                manufacturers.ToDictionary(m => m.Code, m => m.Id, StringComparer.Ordinal),
                kinds.ToDictionary(k => k.Code, k => k.Id, StringComparer.Ordinal),
                models.ToDictionary(m => m.Code, m => m.Id, StringComparer.Ordinal),
                kindParameters.ToDictionary(kp => kp.Code, kp => kp.Id, StringComparer.Ordinal));
        }

        private sealed class EquipmentBatchContext(
            HashSet<int> companyIds,
            HashSet<int> maintenanceEntityIds,
            Dictionary<string, int> manufacturerIdsByCode,
            Dictionary<string, int> kindIdsByCode,
            Dictionary<string, int> modelIdsByCode,
            Dictionary<string, int> kindParameterIdsByCode)
        {
            public HashSet<int> CompanyIds { get; } = companyIds;
            public HashSet<int> MaintenanceEntityIds { get; } = maintenanceEntityIds;
            public Dictionary<string, int> ManufacturerIdsByCode { get; } = manufacturerIdsByCode;
            public Dictionary<string, int> KindIdsByCode { get; } = kindIdsByCode;
            public Dictionary<string, int> ModelIdsByCode { get; } = modelIdsByCode;
            public Dictionary<string, int> KindParameterIdsByCode { get; } = kindParameterIdsByCode;
        }
    }
}