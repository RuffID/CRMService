using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class ModelService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<ModelService> _logger = logger.CreateLogger<ModelService>();

        private async IAsyncEnumerable<List<Model>?> GetModelsFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/models?api_token={okdeskSettings.Value.ApiToken}";

            await foreach (List<Model>? manufacturers in request.GetAllItems<Model>(link, startIndex, limit))
                yield return manufacturers;
        }

        private async Task<List<Model>?> GetModelsFromCloudDb()
        {
            string sqlCommand =
                "SELECT equipment_models.id, equipment_models.code, equipment_models.name, equipment_kinds.code AS kindCode, equipment_manufacturers.code AS manufacturerCode " +
                "FROM equipment_models " +
                "LEFT OUTER JOIN equipment_kinds ON equipment_models.equipment_kind_id = equipment_kinds.id " +
                "LEFT OUTER JOIN equipment_manufacturers ON equipment_models.equipment_manufacturer_id = equipment_manufacturers.id " +
                "ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(model => new Model
                {                    
                    Code = model.Field<string>("code"),
                    Name = model.Field<string>("name"),
                    Kind = new() { Code = model.Field<string>("kindCode") },
                    Manufacturer = new() { Code = model.Field<string>("manufacturerCode") }
                }).ToList();
        }

        public async Task UpdateModelsFromCloudApi(long startIndex, long limit)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating models.", nameof(UpdateModelsFromCloudApi));

            await foreach (List<Model>? models in GetModelsFromCloudApi(startIndex, limit))
            {
                if (models == null || models.Count == 0) return;

                foreach (Model model in models)
                    await CheckModel(model);

                await unitOfWork.Model.CreateOrUpdate(models);

                await unitOfWork.SaveAsync();
            }

            _logger.LogInformation("[Method:{MethodName}] Models update completed.", nameof(UpdateModelsFromCloudApi));
        }

        public async Task UpdateModelsFromCloudDb()
        {
            List<Model>? models = await GetModelsFromCloudDb();

            if (models == null || models.Count == 0)
                return;

            foreach (Model model in models)
                await CheckModel(model);

            await unitOfWork.Model.CreateOrUpdate(models);

            await unitOfWork.SaveAsync();
        }

        private async Task CheckModel(Model model)
        {
            if (model.Manufacturer != null)
                model.ManufacturerId = (await unitOfWork.Manufacturer.GetItem(model.Manufacturer))?.Id;
            if (model.Kind != null)
                model.KindId = (await unitOfWork.Kind.GetItem(model.Kind))?.Id;

            model.Manufacturer = null;
            model.Kind = null;
        }
    }
}