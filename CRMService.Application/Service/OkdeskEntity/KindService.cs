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
    public class KindService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<KindService> logger)
    {
        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<LookupOptionDto>>> GetKindLookupAsync(EquipmentLookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);
            List<int>? modelIds = NormalizeIds(requestModel.ModelIds);

            List<Kind> kinds = await unitOfWork.Kind.GetItemsByPredicateAsync(
                predicate: kind =>
                    (modelIds == null || kind.Models.Any(model => modelIds.Contains(model.Id)))
                    && (normalizedSearch == null
                        || kind.Name.Contains(normalizedSearch)
                        || (kind.Code != null && kind.Code.Contains(normalizedSearch))),
                asNoTracking: true,
                ct: ct);

            IEnumerable<Kind> orderedKinds = normalizedSearch == null
                ? kinds.OrderBy(kind => kind.Id)
                : kinds
                    .OrderBy(kind => GetSearchRank(kind, normalizedSearch))
                    .ThenBy(kind => kind.Name)
                    .ThenBy(kind => kind.Id);

            List<LookupOptionDto> items = orderedKinds
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(kind => new LookupOptionDto
                {
                    Id = kind.Id,
                    Text = kind.Name
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        private async IAsyncEnumerable<List<Kind>> GetKindsFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/kinds?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Kind> kinds in request.GetAllItemsAsync<Kind>(link, startIndex: 0, limit, ct: ct))
                yield return kinds;
        }

        private async Task<List<Kind>?> GetKindsFromCloudDb(CancellationToken ct)
        {
            List<Kind> kinds = await okdeskUnitOfWork.Kind.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return kinds.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateKindsFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kinds from API.", nameof(UpdateKindsFromCloudApi));

            await foreach (List<Kind> kinds in GetKindsFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                if (kinds.Count != 0)
                {
                    foreach (Kind item in kinds)
                    {
                        await sync.RunExclusive(item, async () =>
                        {
                            Kind? existingKinds = await unitOfWork.Kind.GetItemByIdAsync(item.Id, ct: ct);
                            if (existingKinds == null)
                                unitOfWork.Kind.Create(item);
                            else
                                existingKinds.CopyData(item);

                            await unitOfWork.SaveChangesAsync(ct);
                        }, ct);
                    }
                }

                if (kinds.Count < LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API)
                    break;
            }

            logger.LogInformation("[Method:{MethodName}] Update kinds completed.", nameof(UpdateKindsFromCloudApi));
        }

        public async Task UpdateKindsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kinds from DB.", nameof(UpdateKindsFromCloudDb));

            List<Kind>? kinds = await GetKindsFromCloudDb(ct);

            if (kinds != null && kinds.Count != 0)
            {
                foreach (Kind item in kinds)
                {
                    await sync.RunExclusive(item, async () =>
                    {
                        Kind? existingKinds = await unitOfWork.Kind.GetItemByIdAsync(item.Id, ct: ct);
                        if (existingKinds == null)
                            unitOfWork.Kind.Create(item);
                        else
                            existingKinds.CopyData(item);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update kinds completed.", nameof(UpdateKindsFromCloudDb));
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

        private static int GetSearchRank(Kind kind, string search)
        {
            if (kind.Name.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (!string.IsNullOrWhiteSpace(kind.Code) && kind.Code.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (kind.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 2;

            if (!string.IsNullOrWhiteSpace(kind.Code) && kind.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                return 3;

            return 4;
        }
    }
}
