using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Contracts.Models.Responses.Results;
using Microsoft.Extensions.Options;
using System.Data;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Application.Service.Sync;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class IssueStatusService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IPostgresSelect postgresSelect, EntitySyncService sync, ILogger<IssueStatusService> logger)
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
            string sqlCommand = "SELECT * FROM issue_statuses ORDER BY id;;";

            DataSet ds = await postgresSelect.Select(sqlCommand, ct);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return new();

            return table.AsEnumerable().
                Select(status => new IssueStatus
                {
                    Id = status.Field<int>("id"),
                    Code = status.Field<string>("code") ?? "",
                    Name = status.Field<string>("name") ?? string.Empty
                }).ToList();
        }

        public async Task UpdateIssueStatusesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue statuses from API.", nameof(UpdateIssueStatusesFromCloudApi));

            List<IssueStatus> statuses = await GetIssueStatusesFromCloudApi(ct);

            if (statuses.Count == 0)
                return;

            for (int i = 0; i < statuses.Count; i++)
                statuses[i].Id = i + 1;

            foreach (IssueStatus item in statuses)
            {
                await sync.RunExclusive(item, async () =>
                {
                    IssueStatus? existingStatus = await unitOfWork.IssueStatus.GetItemByIdAsync(item.Id, ct: ct);
                    if (existingStatus == null)
                        unitOfWork.IssueStatus.Create(item);
                    else
                        existingStatus.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update issue statuses completed.", nameof(UpdateIssueStatusesFromCloudApi));
        }

        public async Task UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue statuses from DB.", nameof(UpdateIssueStatusesFromCloudDb));

            List<IssueStatus> statuses = await GetIssueStatusesFromCloudDb(ct);

            if (statuses.Count == 0)
                return;

            foreach (IssueStatus item in statuses)
            {
                await sync.RunExclusive(item, async () =>
                {
                    IssueStatus? existingStatus = await unitOfWork.IssueStatus.GetItemByIdAsync(item.Id, ct: ct);
                    if (existingStatus == null)
                        unitOfWork.IssueStatus.Create(item);
                    else
                        existingStatus.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update issue statuses completed.", nameof(UpdateIssueStatusesFromCloudApi));
        }
    }
}