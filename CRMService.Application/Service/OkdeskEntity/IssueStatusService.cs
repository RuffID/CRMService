using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using Microsoft.Extensions.Options;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssueStatusService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<IssueStatusService> logger)
    {
        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<StatusDto>>> GetIssueStatusesAsync(CancellationToken ct)
        {
            List<IssueStatus> statuses = await unitOfWork.IssueStatus.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<StatusDto>>.Ok(statuses.ToDto().ToList());
        }

        public async Task<ServiceResult<List<LookupOptionDto>>> GetIssueStatusLookupAsync(LookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);

            List<IssueStatus> statuses = await unitOfWork.IssueStatus.GetItemsByPredicateAsync(
                predicate: status => normalizedSearch == null || (status.Name != null && status.Name.Contains(normalizedSearch)),
                asNoTracking: true,
                ct: ct);

            IEnumerable<IssueStatus> orderedStatuses = normalizedSearch == null
                ? statuses.OrderBy(status => status.Id)
                : statuses
                    .OrderBy(status => status.Name != null && status.Name.StartsWith(normalizedSearch, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(status => status.Name)
                    .ThenBy(status => status.Id);

            List<LookupOptionDto> items = orderedStatuses
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(status => new LookupOptionDto
                {
                    Id = status.Id,
                    Text = status.Name ?? $"#{status.Id}"
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        public async Task<List<IssueStatus>> GetIssueStatusesFromCloudApi(CancellationToken ct)
        {
            string link = endpoint.Value.OkdeskApi + "/issues/statuses?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItemsAsync<IssueStatus>(link, ct: ct);
        }

        public async Task<List<IssueStatus>> GetIssueStatusesFromCloudDb(CancellationToken ct)
        {
            List<IssueStatus> issueStatuses = await okdeskUnitOfWork.IssueStatus.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return issueStatuses.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateIssueStatusesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue statuses from API.", nameof(UpdateIssueStatusesFromCloudApi));

            List<IssueStatus> statuses = await GetIssueStatusesFromCloudApi(ct);

            if (statuses.Count != 0)
            {
                foreach (IssueStatus item in statuses)
                {
                    await sync.RunExclusive(item, async () =>
                    {
                        IssueStatus? existingStatus = await unitOfWork.IssueStatus.GetItemByPredicateAsync(predicate: s => s.Code == item.Code, ct: ct);
                        if (existingStatus == null)
                        {
                            item.Id = 0;
                            unitOfWork.IssueStatus.Create(item);
                        }
                        else
                            existingStatus.CopyData(item);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update issue statuses completed.", nameof(UpdateIssueStatusesFromCloudApi));
        }

        public async Task UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue statuses from DB.", nameof(UpdateIssueStatusesFromCloudDb));

            List<IssueStatus> statuses = await GetIssueStatusesFromCloudDb(ct);

            if (statuses.Count != 0)
            {
                foreach (IssueStatus item in statuses)
                {
                    await sync.RunExclusive(item, async () =>
                    {
                        IssueStatus? existingStatus = await unitOfWork.IssueStatus.GetItemByPredicateAsync(predicate: s => s.Code == item.Code, ct: ct);
                        if (existingStatus == null)
                        {
                            item.Id = 0;
                            unitOfWork.IssueStatus.Create(item);
                        }
                        else
                            existingStatus.CopyData(item);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update issue statuses completed.", nameof(UpdateIssueStatusesFromCloudApi));
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
    }
}
