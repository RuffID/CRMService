using CRMService.Abstractions.Database.Repository;
using CRMService.DataBase.Postgresql;
using CRMService.Models.ConfigClass;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.OkdeskEntity;
using CRMService.Models.Dto.OkdeskEntity;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.Requests;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;

namespace CRMService.Service.OkdeskEntity
{
    public class ModelService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILogger<ModelService> logger)
    {
        private async IAsyncEnumerable<List<ModelDto>> GetModelsFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/models?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<ModelDto> manufacturers in request.GetAllItems<ModelDto>(link, startIndex: 0, limit, ct: ct))
                yield return manufacturers;
        }

        private async Task<List<Model>?> GetModelsFromCloudDb(CancellationToken ct)
        {
            string sqlCommand =
                "SELECT equipment_models.id, equipment_models.code, equipment_models.name, equipment_kinds.code AS kindCode, equipment_manufacturers.code AS manufacturerCode " +
                "FROM equipment_models " +
                "LEFT OUTER JOIN equipment_kinds ON equipment_models.equipment_kind_id = equipment_kinds.id " +
                "LEFT OUTER JOIN equipment_manufacturers ON equipment_models.equipment_manufacturer_id = equipment_manufacturers.id " +
                "ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand, ct);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(model => new Model
                {
                    Code = model.Field<string>("code") ?? "",
                    Name = model.Field<string>("name") ?? string.Empty,
                    Kind = new() { Code = model.Field<string>("kindCode") ?? "" },
                    Manufacturer = new() { Code = model.Field<string>("manufacturerCode") ?? "" }
                }).ToList();
        }

        public async Task UpdateModelsFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update manufacturers from API.", nameof(UpdateModelsFromCloudApi));

            await foreach (List<ModelDto> models in GetModelsFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (ModelDto model in models)
                {
                    Model? existingModel = await unitOfWork.Model.GetItemByIdAsync(model.Id, ct: ct);
                    if (existingModel == null)
                        unitOfWork.Model.Create(model.ToEntity());
                    else
                        existingModel.CopyData(model.ToEntity());
                }
            }

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateModelsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update manufacturers from DB.", nameof(UpdateModelsFromCloudDb));

            List<Model>? models = await GetModelsFromCloudDb(ct);

            if (models == null || models.Count == 0)
                return;

            foreach (Model model in models)
            {
                await CheckModel(model, ct);

                Model? existingModel = await unitOfWork.Model.GetItemByIdAsync(model.Id, ct: ct);
                if (existingModel == null)
                    unitOfWork.Model.Create(model);
                else
                    existingModel.CopyData(model);
            }

            await unitOfWork.SaveAsync(ct);
        }

        private async Task CheckModel(Model model, CancellationToken ct)
        {
            if (model.Manufacturer != null)
                model.ManufacturerId = (await unitOfWork.Manufacturer.GetItemByPredicateAsync(m => m.Code == model.Manufacturer.Code, asNoTracking: true, ct: ct))?.Id ?? 0;
            if (model.Kind != null)
                model.KindId = (await unitOfWork.Kind.GetItemByPredicateAsync(k => k.Code == model.Kind.Code, asNoTracking: true, ct: ct))?.Id ?? 0;
        }
    }
}