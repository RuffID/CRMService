using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class EquipmentService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, 
        IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<EquipmentService> _logger = logger.CreateLogger<EquipmentService>();

        private async IAsyncEnumerable<List<Equipment>?> GetEquipmentsFromCloudApi(long startIndex, long limit, long companyId = 0, long maintenanceEntityId = 0)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.OkdeskApiToken}";

            if (companyId > 0)
                link += $"&company_ids[]={companyId}";
            if (maintenanceEntityId > 0)
                link += $"&maintenance_entity_ids[]={maintenanceEntityId}";

            await foreach (var equipments in request.GetAllItems<Equipment>(link, startIndex, limit))
                yield return equipments;
        }

        private async Task<List<Equipment>?> GetEquipmentsFromCloudDb(int startIndex, int limit)
        {
            string sqlCommand = string.Format(
                    "SELECT equipments.sequential_id, equipments.inventory_number, equipments.serial_number, " +
                    "equipments.parameters, company_maintenance_entities.sequential_id AS maintenanceEntitiesId, " +
                    "companies.sequential_id AS companyId, equipment_kinds.id AS kindId, " +
                    "equipment_manufacturers.id AS manufacturerId, equipment_models.id AS modelId " +
                    "FROM equipments " +
                    "LEFT OUTER JOIN companies ON equipments.company_id = companies.id " +
                    "LEFT OUTER JOIN company_maintenance_entities ON equipments.maintenance_entity_id = company_maintenance_entities.id " +
                    "LEFT OUTER JOIN equipment_kinds ON equipments.equipment_kind_id = equipment_kinds.id " +
                    "LEFT OUTER JOIN equipment_manufacturers ON equipments.equipment_manufacturer_id = equipment_manufacturers.id " +
                    "LEFT OUTER JOIN equipment_models ON equipments.equipment_model_id = equipment_models.id " +
                    "WHERE equipments.sequential_id > '{0}' ORDER BY equipments.sequential_id LIMIT '{1}';", startIndex, limit);

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(equipment => new Equipment
                {
                    Id = equipment.Field<int>("sequential_id"),
                    SerialNumber = equipment.Field<string?>("serial_number"),
                    InventoryNumber = equipment.Field<string?>("inventory_number"),
                    CompanyId = equipment.Field<int?>("companyId"),
                    MaintenanceEntitiesId = equipment.Field<int?>("maintenanceEntitiesId"),
                    KindId = equipment.Field<int?>("kindId"),
                    ManufacturerId = equipment.Field<int?>("manufacturerId"),
                    ModelId = equipment.Field<int?>("modelId"),
                    Parameters = HandleParameters(equipment.Field<string>("parameters"))
                }).ToList();
        }

        public async Task UpdateEquipmentFromCloudApi(long id, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.OkdeskApiToken}&page[from_id]={id}&page[direction]=forward&page[size]=1";

            List<Equipment>? equipments = await request.GetRangeOfItems<Equipment>(link);

            if (equipments == null || equipments.Count == 0)
                return;

            Equipment? equipment = equipments.First();

            await unitOfWork.Equipment.Upsert(equipment, ct);

            if (equipment.Parameters != null && equipment.Parameters.Count > 0)
                await unitOfWork.Parameter.Upsert(equipment.Parameters, p => i => i.KindParameterId == p.KindParameterId && i.EquipmentId == p.EquipmentId, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateEquipmentsFromCloudApi(long startIndex, long limit, long companyId = 0, long maintenanceEntityId = 0, CancellationToken ct = default)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating equipments.", nameof(UpdateEquipmentsFromCloudApi));

            await foreach (List<Equipment>? equipments in GetEquipmentsFromCloudApi(startIndex, limit, companyId, maintenanceEntityId))
            {
                if (equipments == null || equipments.Count == 0)
                    return;

                await unitOfWork.Equipment.Upsert(equipments, ct);

                foreach (var equipment in equipments)
                {
                    if (equipment.Parameters != null && equipment.Parameters.Count > 0)
                        await unitOfWork.Parameter.Upsert(equipment.Parameters, p => (EquipmentParameter i) => i.KindParameterId == p.KindParameterId && i.EquipmentId == p.EquipmentId, ct);
                }

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Equipments update completed.", nameof(UpdateEquipmentsFromCloudApi));
        }

        public async Task UpdateEquipmentsFromCloudDb(int startIndex, int limit, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating equipments.", nameof(UpdateEquipmentsFromCloudDb));

            while (true)
            {
                List<Equipment>? equipments = await GetEquipmentsFromCloudDb(startIndex, limit);

                if (equipments == null || equipments.Count == 0)
                    return;

                startIndex = equipments.Last().Id;

                await unitOfWork.Equipment.Upsert(equipments, ct);

                foreach (Equipment equipment in equipments)
                {
                    if (equipment.Parameters != null && equipment.Parameters.Count > 0)
                        await unitOfWork.Parameter.Upsert(equipment.Parameters, p => (EquipmentParameter i) => i.KindParameterId == p.KindParameterId && i.EquipmentId == p.EquipmentId, ct);
                }

                await unitOfWork.SaveAsync(ct);

                if (equipments.Count < limit)
                    break;
            }

            _logger.LogInformation("[Method:{MethodName}] Equipments update completed.", nameof(UpdateEquipmentsFromCloudDb));
        }        

        private static List<EquipmentParameter> HandleParameters(string? parameters)
        {
            List<EquipmentParameter> result = new List<EquipmentParameter>();
            if (string.IsNullOrEmpty(parameters)) 
                return result;

            JObject paramObject = JObject.Parse(parameters);
            return paramObject.Properties().Select(parameter => new EquipmentParameter
            {
                Value = parameter.Value.ToString()
            }).ToList();
        }
    }
}