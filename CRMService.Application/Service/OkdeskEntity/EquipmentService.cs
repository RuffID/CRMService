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

                foreach (Equipment equipment in equipments)
                {
                    await sync.RunExclusive(equipment, async () =>
                    {
                        await CreateOrUpdate(equipment, ct);
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
            await CheckInformationOnEquipment(equipment, ct);

            Equipment? existingEquipment = await unitOfWork.Equipment.GetItemByIdAsync(equipment.Id, ct: ct);

            if (existingEquipment == null)
                unitOfWork.Equipment.Create(equipment);
            else
                existingEquipment.CopyData(equipment);

            foreach (EquipmentParameter parameter in equipment.Parameters)
            {
                EquipmentParameter? existingParameter = await unitOfWork.Parameter.GetItemByPredicateAsync(p => p.EquipmentId == parameter.EquipmentId && p.KindParameterId == parameter.KindParameterId, ct: ct);

                if (existingParameter == null)
                    unitOfWork.Parameter.Create(parameter);
                else
                    existingParameter.CopyData(parameter);
            }
            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task CheckInformationOnEquipment(Equipment equipment, CancellationToken ct)
        {
            if (equipment.Company != null)
                equipment.Company = await unitOfWork.Company.GetItemByIdAsync(equipment.Company.Id, ct: ct);
            else if (equipment.CompanyId != null)
                equipment.Company = await unitOfWork.Company.GetItemByIdAsync(equipment.CompanyId.Value, ct: ct);

            if (equipment.MaintenanceEntities != null)
                equipment.MaintenanceEntities = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(equipment.MaintenanceEntities.Id, ct: ct);
            else if (equipment.MaintenanceEntitiesId != null)
                equipment.MaintenanceEntities = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(equipment.MaintenanceEntitiesId.Value, ct: ct);

            if (equipment.Manufacturer != null)
                equipment.Manufacturer = await unitOfWork.Manufacturer.GetItemByPredicateAsync(m => m.Code == equipment.Manufacturer.Code, ct: ct);

            if (equipment.Kind != null)
                equipment.Kind = await unitOfWork.Kind.GetItemByPredicateAsync(k => k.Code == equipment.Kind.Code, ct: ct);

            if (equipment.Model != null)
                equipment.Model = await unitOfWork.Model.GetItemByPredicateAsync(m => m.Code == equipment.Model.Code, ct: ct);

            if (equipment.Parameters != null && equipment.Parameters.Count != 0)
            {
                equipment.Parameters.RemoveAll(p => string.IsNullOrEmpty(Convert.ToString(p.Value)));

                foreach (EquipmentParameter parameter in equipment.Parameters)
                {
                    parameter.EquipmentId = equipment.Id;
                    parameter.KindParameterId = (await unitOfWork.KindParameter.GetItemByPredicateAsync(predicate: kp => kp.Code == parameter.Code, true, ct: ct))?.Id
                        ?? throw new InvalidOperationException($"Equipment parameter: {parameter.Code} - not found.");
                }
            }
        }

    }
}