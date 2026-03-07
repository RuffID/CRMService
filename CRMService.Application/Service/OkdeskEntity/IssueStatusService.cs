using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Contracts.Models.Responses.Results;
using Microsoft.Extensions.Options;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Service.Sync;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssueStatusService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, EntitySyncService sync, ILogger<IssueStatusService> logger)
    {
        public async Task<ServiceResult<List<StatusDto>>> GetIssueStatusesAsync(CancellationToken ct)
        {
            List<IssueStatus> statuses = await unitOfWork.IssueStatus.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<StatusDto>>.Ok(statuses.ToDto().ToList());
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
    }
}