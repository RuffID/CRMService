using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.OkdeskEntity.Resolvers;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
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
        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<LookupOptionDto>>> GetModelLookupAsync(EquipmentLookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);
            List<int>? typeIds = NormalizeIds(requestModel.TypeIds);
            List<int>? manufacturerIds = NormalizeIds(requestModel.ManufacturerIds);

            List<Model> models = await unitOfWork.Model.GetItemsByPredicateAsync(
                predicate: model =>
                    (typeIds == null || (model.KindId.HasValue && typeIds.Contains(model.KindId.Value)))
                    && (manufacturerIds == null || (model.ManufacturerId.HasValue && manufacturerIds.Contains(model.ManufacturerId.Value)))
                    && (normalizedSearch == null
                        || model.Name.Contains(normalizedSearch)
                        || (model.Code != null && model.Code.Contains(normalizedSearch))
                        || (model.Manufacturer != null && model.Manufacturer.Name.Contains(normalizedSearch))),
                asNoTracking: true,
                include: query => query.Include(model => model.Manufacturer),
                ct: ct);

            IEnumerable<Model> orderedModels = normalizedSearch == null
                ? models.OrderBy(model => model.Id)
                : models
                    .OrderBy(model => GetSearchRank(model, normalizedSearch))
                    .ThenBy(model => model.Name)
                    .ThenBy(model => model.Id);

            List<LookupOptionDto> items = orderedModels
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(model => new LookupOptionDto
                {
                    Id = model.Id,
                    Text = model.Manufacturer == null || string.IsNullOrWhiteSpace(model.Manufacturer.Name)
                        ? model.Name
                        : $"{model.Name} ({model.Manufacturer.Name})"
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

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

        private static int GetSearchRank(Model model, string search)
        {
            if (model.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (!string.IsNullOrWhiteSpace(model.Manufacturer?.Name) && model.Manufacturer.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 2;

            if (model.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 3;

            if (!string.IsNullOrWhiteSpace(model.Manufacturer?.Name) && model.Manufacturer.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 4;

            if (!string.IsNullOrWhiteSpace(model.Code) && model.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 5;

            return 6;
        }
    }
}
