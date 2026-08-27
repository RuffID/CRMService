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
    public class ManufacturerService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<ManufacturerService> logger)
    {
        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<LookupOptionDto>>> GetManufacturerLookupAsync(EquipmentLookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);
            List<int>? modelIds = NormalizeIds(requestModel.ModelIds);

            List<Manufacturer> manufacturers = await unitOfWork.Manufacturer.GetItemsByPredicateAsync(
                predicate: manufacturer =>
                    (modelIds == null || manufacturer.Models.Any(model => modelIds.Contains(model.Id)))
                    && (normalizedSearch == null
                        || manufacturer.Name.Contains(normalizedSearch)
                        || (manufacturer.Code != null && manufacturer.Code.Contains(normalizedSearch))),
                asNoTracking: true,
                ct: ct);

            IEnumerable<Manufacturer> orderedManufacturers = normalizedSearch == null
                ? manufacturers.OrderBy(manufacturer => manufacturer.Id)
                : manufacturers
                    .OrderBy(manufacturer => GetSearchRank(manufacturer, normalizedSearch))
                    .ThenBy(manufacturer => manufacturer.Name)
                    .ThenBy(manufacturer => manufacturer.Id);

            List<LookupOptionDto> items = orderedManufacturers
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(manufacturer => new LookupOptionDto
                {
                    Id = manufacturer.Id,
                    Text = manufacturer.Name
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        private async IAsyncEnumerable<List<Manufacturer>> GetManufacturersFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/manufacturers?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Manufacturer> manufacturers in request.GetAllItemsAsync<Manufacturer>(link, startIndex: 0, limit, ct: ct))
                yield return manufacturers;
        }

        private async Task<List<Manufacturer>> GetManufacturersFromCloudDb(CancellationToken ct)
        {
            List<Manufacturer> manufacturers = await okdeskUnitOfWork.Manufacturer.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);
            
            return manufacturers.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateManufacturersFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update model from API.", nameof(UpdateManufacturersFromCloudApi));

            await foreach (List<Manufacturer> manufacturers in GetManufacturersFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (Manufacturer newManufacturer in manufacturers)
                {
                    await sync.RunExclusive(newManufacturer, async () =>
                    {
                        Manufacturer? existingManufacturer = await unitOfWork.Manufacturer.GetItemByIdAsync(newManufacturer.Id, ct: ct);
                        if (existingManufacturer == null)
                            unitOfWork.Manufacturer.Create(newManufacturer);
                        else
                            existingManufacturer.CopyData(newManufacturer);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update model completed.", nameof(UpdateManufacturersFromCloudApi));
        }

        public async Task UpdateManufacturersFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update manufacturers from DB.", nameof(UpdateManufacturersFromCloudDb));

            List<Manufacturer> manufacturers = await GetManufacturersFromCloudDb(ct);

            foreach (Manufacturer newManufacturer in manufacturers)
            {
                await sync.RunExclusive(newManufacturer, async () =>
                {
                    Manufacturer? existingManufacturer = await unitOfWork.Manufacturer.GetItemByIdAsync(newManufacturer.Id, ct: ct);
                    if (existingManufacturer == null)
                        unitOfWork.Manufacturer.Create(newManufacturer);
                    else
                        existingManufacturer.CopyData(newManufacturer);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update model completed.", nameof(UpdateManufacturersFromCloudDb));
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

        private static int GetSearchRank(Manufacturer manufacturer, string search)
        {
            if (manufacturer.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (!string.IsNullOrWhiteSpace(manufacturer.Code) && manufacturer.Code.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (manufacturer.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 2;

            if (!string.IsNullOrWhiteSpace(manufacturer.Code) && manufacturer.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 3;

            return 4;
        }
    }
}
