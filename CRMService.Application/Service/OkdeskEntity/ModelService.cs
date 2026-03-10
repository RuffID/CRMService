using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity.Resolvers;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class ModelService(
        IOptions<ApiEndpointOptions> endpoint,
        IOptions<OkdeskOptions> okdeskSettings,
        IOkdeskEntityRequestService request,
        IUnitOfWork unitOfWork,
        IOkdeskUnitOfWork okdeskUnitOfWork,
        EntitySyncService sync,
        KindResolverService kindResolver,
        ManufacturerResolverService manufacturerResolver,
        ILogger<ModelService> logger)
    {
        private async IAsyncEnumerable<List<Model>> GetModelsFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/models?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Model> models in request.GetAllItemsAsync<Model>(link, startIndex: 0, limit, ct: ct))
                yield return models;
        }

        private async Task<List<Model>> GetModelsFromCloudDb(CancellationToken ct)
        {
            List<Model> models = await okdeskUnitOfWork.Model.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return models.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateModelsFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update models from API.", nameof(UpdateModelsFromCloudApi));

            await foreach (List<Model> modelsFromApi in GetModelsFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                if (modelsFromApi.Count != 0)
                {
                    foreach (Model model in modelsFromApi)
                    {
                        await sync.RunExclusive(model, async () =>
                        {
                            model.KindId = model.Kind?.Id;
                            model.ManufacturerId = model.Manufacturer?.Id;

                            await CheckModel(model, ct);

                            Model? existingModel = await unitOfWork.Model.GetItemByIdAsync(model.Id, ct: ct);
                            if (existingModel == null)
                            {
                                model.Kind = null;
                                model.Manufacturer = null;
                                unitOfWork.Model.Create(model);
                            }
                            else
                                existingModel.CopyData(model);

                            await unitOfWork.SaveChangesAsync(ct);
                        }, ct);
                    }
                }

                if (modelsFromApi.Count < LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}] Update models completed.", nameof(UpdateModelsFromCloudApi));
        }

        public async Task UpdateModelsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update manufacturers from DB.", nameof(UpdateModelsFromCloudDb));

            List<Model> models = await GetModelsFromCloudDb(ct);

            if (models.Count != 0)
            {
                foreach (Model model in models)
                {
                    await sync.RunExclusive(model, async () =>
                    {
                        await CheckModel(model, ct);

                        Model? existingModel = await unitOfWork.Model.GetItemByIdAsync(model.Id, ct: ct);
                        if (existingModel == null)
                            unitOfWork.Model.Create(model);
                        else
                            existingModel.CopyData(model);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update models completed.", nameof(UpdateModelsFromCloudDb));
        }

        private async Task CheckModel(Model model, CancellationToken ct)
        {
            if (model.Manufacturer != null)
                model.ManufacturerId = await manufacturerResolver.ResolveManufacturerIdAsync(model.Manufacturer, "model", model.Id, ct);

            if (model.Kind != null)
                model.KindId = await kindResolver.ResolveKindIdAsync(model.Kind, "model", model.Id, ct);
        }
    }
}