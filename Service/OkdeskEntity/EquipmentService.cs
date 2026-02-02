using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class EquipmentService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request,
        IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<EquipmentService> _logger = logger.CreateLogger<EquipmentService>();

        private async IAsyncEnumerable<List<Equipment>> GetEquipmentsFromCloudApi(long startIndex, long limit, long companyId = 0, long maintenanceEntityId = 0)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.OkdeskApiToken}";

            if (companyId > 0)
                link += $"&company_ids[]={companyId}";
            if (maintenanceEntityId > 0)
                link += $"&maintenance_entity_ids[]={maintenanceEntityId}";

            await foreach (List<Equipment> equipments in request.GetAllItems<Equipment>(link, startIndex, limit))
                yield return equipments;
        }

        private async Task<List<Equipment>> GetEquipmentsFromCloudDb(int startIndex, int limit)
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
                return new();

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
                    Parameters = HandleParameters(equipment.Field<int>("sequential_id"), equipment.Field<string>("parameters"))
                }).ToList();
        }

        public async Task UpdateEquipmentFromCloudApi(long id, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.OkdeskApiToken}&page[from_id]={id}&page[direction]=forward&page[size]=1";

            List<Equipment>? equipments = await request.GetRangeOfItems<Equipment>(link);

            if (equipments == null || equipments.Count == 0)
                return;

            Equipment? equipment = equipments.First();

            await CheckInformationOnEquipment(equipment, ct);

            foreach (EquipmentParameter parameter in equipment.Parameters)
            {
                EquipmentParameter? existingParameter = await unitOfWork.Parameter.GetItemByPredicateAsync(p => p.EquipmentId == parameter.EquipmentId && p.KindParameterId == parameter.KindParameterId, ct: ct);

                if (existingParameter == null)
                    unitOfWork.Parameter.Create(parameter);
                else
                    existingParameter.CopyData(parameter);
            }

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateEquipmentsFromCloudApi(long startIndex, long limit, long companyId = 0, long maintenanceEntityId = 0, CancellationToken ct = default)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating equipments.", nameof(UpdateEquipmentsFromCloudApi));

            await foreach (List<Equipment> equipments in GetEquipmentsFromCloudApi(startIndex, limit, companyId, maintenanceEntityId))
            {
                if (equipments.Count == 0)
                    return;

                foreach (Equipment equipment in equipments)
                {
                    await CheckInformationOnEquipment(equipment, ct);

                    foreach (EquipmentParameter parameter in equipment.Parameters)
                    {
                        EquipmentParameter? existingParameter = await unitOfWork.Parameter.GetItemByPredicateAsync(p => p.EquipmentId == parameter.EquipmentId && p.KindParameterId == parameter.KindParameterId, ct: ct);

                        if (existingParameter == null)
                            unitOfWork.Parameter.Create(parameter);
                        else
                            existingParameter.CopyData(parameter);
                    }
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
                List<Equipment> equipments = await GetEquipmentsFromCloudDb(startIndex, limit);

                if (equipments.Count == 0)
                    return;

                startIndex = equipments.Last().Id;

                foreach (Equipment equipment in equipments)
                {
                    await CheckInformationOnEquipment(equipment, ct);

                    foreach (EquipmentParameter parameter in equipment.Parameters)
                    {
                        EquipmentParameter? existingParameter = await unitOfWork.Parameter.GetItemByPredicateAsync(p => p.EquipmentId == parameter.EquipmentId && p.KindParameterId == parameter.KindParameterId, ct: ct);

                        if (existingParameter == null)
                            unitOfWork.Parameter.Create(parameter);
                        else
                            existingParameter.CopyData(parameter);
                    }
                }

                await unitOfWork.SaveAsync(ct);

                if (equipments.Count < limit)
                    break;
            }

            _logger.LogInformation("[Method:{MethodName}] Equipments update completed.", nameof(UpdateEquipmentsFromCloudDb));
        }

        public async Task CheckInformationOnEquipment(Equipment equipment, CancellationToken ct)
        {
            if (equipment.Company != null)
                equipment.CompanyId = (await unitOfWork.Company.GetItemByIdAsync(equipment.Company.Id, true, ct: ct))?.Id;
            else if (equipment.CompanyId != null)
                equipment.CompanyId = (await unitOfWork.Company.GetItemByIdAsync(equipment.CompanyId.Value, true))?.Id;

            if (equipment.MaintenanceEntities != null)
                equipment.MaintenanceEntitiesId = (await unitOfWork.MaintenanceEntity.GetItemByIdAsync(equipment.MaintenanceEntities.Id, true, ct: ct))?.Id;
            else if (equipment.MaintenanceEntitiesId != null)
                equipment.MaintenanceEntitiesId = (await unitOfWork.MaintenanceEntity.GetItemByIdAsync(equipment.MaintenanceEntitiesId.Value, true, ct: ct))?.Id;

            if (equipment.Manufacturer != null)
                equipment.ManufacturerId = (await unitOfWork.Manufacturer.GetItemByPredicateAsync(m => m.Code == equipment.Manufacturer.Code, true, ct: ct))?.Id;

            if (equipment.Kind != null)
                equipment.KindId = (await unitOfWork.Kind.GetItemByPredicateAsync(k => k.Code == equipment.Kind.Code, true, ct: ct))?.Id;

            if (equipment.Model != null)
                equipment.ModelId = (await unitOfWork.Model.GetItemByPredicateAsync(m => m.Code == equipment.Model.Code, true, ct: ct))?.Id;

            if (equipment.Parameters != null && equipment.Parameters.Count != 0)
            {
                equipment.Parameters.RemoveAll(p => string.IsNullOrEmpty(Convert.ToString(p.Value)));

                foreach (EquipmentParameter parameter in equipment.Parameters)
                {
                    parameter.EquipmentId = equipment.Id;
                    parameter.KindParameterId = (await unitOfWork.KindParameter.GetItemByPredicateAsync(predicate: kp => kp.Code == parameter.Code, true, ct: ct))?.Id
                        ?? throw new InvalidOperationException($"Equipment parameter: {parameter.Code} - not found.");
                }

                equipment.Parameters.RemoveAll(p => p.KindParameterId == null);
            }

            equipment.Company = null;
            equipment.MaintenanceEntities = null;
            equipment.Manufacturer = null;
            equipment.Kind = null;
            equipment.Model = null;
        }

        private static List<EquipmentParameter> HandleParameters(int equipmentId, string? parameters)
        {
            List<EquipmentParameter> result = new();
            if (string.IsNullOrEmpty(parameters))
                return result;

            JToken token = JToken.Parse(parameters);

            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty p in ((JObject)token).Properties())
                {
                    string? valueStr = p.Value.Type switch
                    {
                        JTokenType.Null => null,
                        JTokenType.String => p.Value.Value<string>(),
                        _ => p.Value.ToString(Formatting.None)
                    };

                    if (string.IsNullOrEmpty(valueStr))
                        continue;

                    result.Add(new EquipmentParameter
                    {
                        Code = p.Name,
                        Value = valueStr,
                        EquipmentId = equipmentId
                    });
                }
                return result;
            }

            result.Add(new EquipmentParameter
            {
                Code = null,
                Value = token.Type == JTokenType.Array || token.Type == JTokenType.Object
                    ? token.ToString(Formatting.None)
                    : (token as JValue)?.Value,
                EquipmentId = equipmentId
            });

            return result;
        }
    }
}