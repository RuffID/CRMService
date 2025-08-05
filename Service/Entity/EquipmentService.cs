using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Data;

namespace CRMService.Service.Entity
{
    public class EquipmentService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, 
        IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<EquipmentService> _logger = logger.CreateLogger<EquipmentService>();

        private async IAsyncEnumerable<List<Equipment>?> GetEquipmentsFromCloudApi(long startIndex, long limit, long companyId = 0, long maintenanceEntityId = 0)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.ApiToken}";

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
                    "companies.sequential_id AS companyId, equipment_kinds.code AS kindCode, " +
                    "equipment_manufacturers.code AS manufacturerCode, equipment_models.code AS modelCode " +
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
                    Kind = new() { Code = equipment.Field<string?>("kindCode") },
                    Manufacturer = new() { Code = equipment.Field<string?>("manufacturerCode") },
                    Model = new() { Code = equipment.Field<string?>("modelCode") },
                    Parameters = HandleParameters(equipment.Field<string?>("parameters"))
                }).ToList();
        }

        public async Task UpdateEquipmentFromCloudApi(long id)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/list?api_token={okdeskSettings.Value.ApiToken}&page[from_id]={id}&page[direction]=forward&page[size]=1";

            List<Equipment>? equipments = await request.GetRangeOfItems<Equipment>(link);

            if (equipments == null || equipments.Count == 0)
                return;

            Equipment? equipment = equipments.First();

            await CheckInformationOnEquipment(equipment);

            await unitOfWork.Equipment.CreateOrUpdate([equipment]);

            if (equipment.Parameters != null && equipment.Parameters.Count > 0)
                await unitOfWork.Parameter.CreateOrUpdate(equipment.Parameters);

            await unitOfWork.SaveAsync();
        }

        public async Task UpdateEquipmentsFromCloudApi(long startIndex, long limit, long companyId = 0, long maintenanceEntityId = 0)
        {
            await foreach (List<Equipment>? equipments in GetEquipmentsFromCloudApi(startIndex, limit, companyId, maintenanceEntityId))
            {
                if (equipments == null || equipments.Count == 0)
                    return;

                foreach (var equipment in equipments)
                    await CheckInformationOnEquipment(equipment);

                await unitOfWork.Equipment.CreateOrUpdate(equipments);

                foreach (var equipment in equipments)
                {
                    if (equipment.Parameters != null && equipment.Parameters.Count > 0)
                        await unitOfWork.Parameter.CreateOrUpdate(equipment.Parameters);
                }

                await unitOfWork.SaveAsync();
            }
        }

        public async Task UpdateEquipmentsFromCloudDb(int startIndex, int limit)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating equipments.", nameof(UpdateEquipmentsFromCloudDb));

            while (true)
            {
                List<Equipment>? equipments = await GetEquipmentsFromCloudDb(startIndex, limit);

                if (equipments == null || equipments.Count == 0)
                    return;

                startIndex = equipments.Last().Id;

                foreach (var equipment in equipments)
                    await CheckInformationOnEquipment(equipment);

                await unitOfWork.Equipment.CreateOrUpdate(equipments);

                foreach (var equipment in equipments)
                {
                    if (equipment.Parameters != null && equipment.Parameters.Count > 0)
                        await unitOfWork.Parameter.CreateOrUpdate(equipment.Parameters);
                }

                await unitOfWork.SaveAsync();

                if (equipments.Count < limit)
                    break;
            }

            _logger.LogInformation("[Method:{MethodName}] Equipments update completed.", nameof(UpdateEquipmentsFromCloudDb));
        }

        public async Task CheckInformationOnEquipment(Equipment equipment)
        {
            if (equipment.Company != null)
                equipment.CompanyId = (await unitOfWork.Company.GetItem(equipment.Company, false))?.Id;
            else if (equipment.CompanyId != null)
                equipment.CompanyId = (await unitOfWork.Company.GetCompanyById((int)equipment.CompanyId, false))?.Id;

            if (equipment.MaintenanceEntities != null)
                equipment.MaintenanceEntitiesId = (await unitOfWork.MaintenanceEntity.GetItem(equipment.MaintenanceEntities, false))?.Id;
            else if (equipment.MaintenanceEntitiesId != null)
                equipment.MaintenanceEntitiesId = (await unitOfWork.MaintenanceEntity.GetMaintenanceEntityById((int)equipment.MaintenanceEntitiesId, false))?.Id;

            if (equipment.Manufacturer != null)
                equipment.ManufacturerId = (await unitOfWork.Manufacturer.GetItem(equipment.Manufacturer, false))?.Id;

            if (equipment.Kind != null)
                equipment.KindId = (await unitOfWork.Kind.GetItem(equipment.Kind, false))?.Id;

            if (equipment.Model != null)
                equipment.ModelId = (await unitOfWork.Model.GetItem(equipment.Model, false))?.Id;

            if (equipment.Parameters != null && equipment.Parameters.Count != 0)
            {
                equipment.Parameters.RemoveAll(p => string.IsNullOrEmpty(Convert.ToString(p.Value)));

                foreach (var parameter in equipment.Parameters)
                {
                    parameter.EquipmentId = equipment.Id;
                    parameter.KindParameterId = (await unitOfWork.KindParameter.GetItem(new KindsParameter() { Code = parameter.Code }, false))?.Id;
                }

                equipment.Parameters.RemoveAll(p => p.KindParameterId == null);
            }

            equipment.Company = null;
            equipment.MaintenanceEntities = null;
            equipment.Manufacturer = null;
            equipment.Kind = null;
            equipment.Model = null;
        }

        private static List<Parameter>? HandleParameters(string? parameters)
        {
            if (string.IsNullOrEmpty(parameters)) return null;

            JObject paramObject = JObject.Parse(parameters);
            return paramObject.Properties().Select(parameter => new Parameter
            {
                Code = parameter.Name,
                Value = parameter.Value.ToString()
            }).ToList();
        }
    }
}