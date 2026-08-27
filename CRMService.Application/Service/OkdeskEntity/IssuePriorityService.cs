using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Contracts.Models.Responses.Results;
using Microsoft.Extensions.Options;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Service.Sync;
using CRMService.Contracts.Models.Dto.Lookup;
using CRMService.Contracts.Models.Request;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssuePriorityService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IIssuesUnitOfWork unitOfWork, IOkdeskIssuesSource okdeskUnitOfWork, EntitySyncService sync, ILogger<IssuePriorityService> logger)
    {
        public Task<IssuePriority?> GetIssuePriorityAsync(string code, CancellationToken ct = default) =>
            unitOfWork.IssuePriority.GetByCodeReadOnlyAsync(code, ct);

        private const int DEFAULT_LOOKUP_LIMIT = 20;

        public async Task<ServiceResult<List<PriorityDto>>> GetIssuePrioritiesAsync(CancellationToken ct)
        {
            List<IssuePriority> priorities = await unitOfWork.IssuePriority.GetItemsReadOnlyAsync(ct);

            return ServiceResult<List<PriorityDto>>.Ok(priorities.ToDto().ToList());
        }

        public async Task<ServiceResult<List<LookupOptionDto>>> GetIssuePriorityLookupAsync(LookupListRequest requestModel, CancellationToken ct = default)
        {
            ServiceResult validationResult = ValidateLookupRequest(requestModel);
            if (!validationResult.Success)
                return ServiceResult<List<LookupOptionDto>>.Fail(validationResult.Error!.StatusCode, validationResult.Error.Message);

            string? normalizedSearch = NormalizeSearch(requestModel.Search);

            List<IssuePriority> priorities = await unitOfWork.IssuePriority.SearchReadOnlyAsync(normalizedSearch, ct);

            IEnumerable<IssuePriority> orderedPriorities = normalizedSearch == null
                ? priorities.OrderBy(priority => priority.Id)
                : priorities
                    .OrderBy(priority => priority.Name != null && priority.Name.StartsWith(normalizedSearch, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(priority => priority.Name)
                    .ThenBy(priority => priority.Id);

            List<LookupOptionDto> items = orderedPriorities
                .Skip(requestModel.Offset)
                .Take(requestModel.Limit)
                .Select(priority => new LookupOptionDto
                {
                    Id = priority.Id,
                    Text = priority.Name ?? $"#{priority.Id}"
                })
                .ToList();

            return ServiceResult<List<LookupOptionDto>>.Ok(items);
        }

        public async Task<List<IssuePriority>> GetIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            string link = endpoint.Value.OkdeskApi + "/issues/priorities?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItemsAsync<IssuePriority>(link, ct: ct);
        }

        public async Task<List<IssuePriority>> GetIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            List<IssuePriority> issuePriorities = await okdeskUnitOfWork.IssuePriority.GetAllReadOnlyAsync(ct);

            return issuePriorities.OrderBy(x => x.Id).ToList();
        }

        public async Task UpdateIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue priorities.", nameof(UpdateIssuePrioritiesFromCloudApi));

            List<IssuePriority> priorities = await GetIssuePrioritiesFromCloudApi(ct);

            if (priorities.Count != 0)
            {
                foreach (IssuePriority priority in priorities)
                {
                    await sync.RunExclusive(priority, async () =>
                    {
                        IssuePriority? existingPriority = await unitOfWork.IssuePriority.GetByCodeAsync(priority.Code, ct);
                        if (existingPriority == null)
                        {
                            priority.Id = 0;
                            unitOfWork.IssuePriority.Create(priority);
                        }
                        else
                            existingPriority.CopyData(priority);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Issue priorities update completed.", nameof(UpdateIssuePrioritiesFromCloudApi));
        }

        public async Task UpdateIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue priorities.", nameof(UpdateIssuePrioritiesFromCloudDb));

            List<IssuePriority> priorities = await GetIssuePrioritiesFromCloudDb(ct);

            if (priorities.Count != 0)
            {
                foreach (IssuePriority priority in priorities)
                {
                    await sync.RunExclusive(priority, async () =>
                    {
                        IssuePriority? existingPriority = await unitOfWork.IssuePriority.GetByCodeAsync(priority.Code, ct);
                        if (existingPriority == null)
                        {
                            priority.Id = 0;
                            unitOfWork.IssuePriority.Create(priority);
                        }
                        else
                            existingPriority.CopyData(priority);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update issue priorities completed.", nameof(UpdateIssuePrioritiesFromCloudDb));
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
