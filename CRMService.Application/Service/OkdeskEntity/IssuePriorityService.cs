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
    public class IssuePriorityService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IPostgresSelect postgresSelect, EntitySyncService sync, ILogger<IssuePriorityService> logger)
    {
        public async Task<ServiceResult<List<PriorityDto>>> GetIssuePrioritiesAsync(CancellationToken ct)
        {
            List<IssuePriority> priorities = await unitOfWork.IssuePriority.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return ServiceResult<List<PriorityDto>>.Ok(priorities.ToDto().ToList());
        }

        public async Task<List<IssuePriority>> GetIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            string link = endpoint.Value.OkdeskApi + "/issues/priorities?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItemsAsync<IssuePriority>(link, ct: ct);
        }

        public async Task<List<IssuePriority>> GetIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            string sqlCommand = "SELECT * FROM issue_priorities ORDER BY id;";

            DataSet ds = await postgresSelect.Select(sqlCommand, ct);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return new();

            return table.AsEnumerable().
                Select(priority => new IssuePriority
                {
                    Id = priority.Field<int>("id"),
                    Code = priority.Field<string>("code") ?? "",
                    Name = priority.Field<string>("name")
                }).ToList();
        }

        public async Task UpdateIssuePrioritiesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update issue priorities.", nameof(UpdateIssuePrioritiesFromCloudApi));

            List<IssuePriority> priorities = await GetIssuePrioritiesFromCloudApi(ct);

            if (priorities.Count != 0)
            {
                for (int i = 0; i < priorities.Count; i++)
                    priorities[i].Id = i + 1;

                foreach (IssuePriority priority in priorities)
                {
                    await sync.RunExclusive(priority, async () =>
                    {
                        IssuePriority? existingPriority = await unitOfWork.IssuePriority.GetItemByIdAsync(priority.Id, ct: ct);
                        if (existingPriority == null)
                            unitOfWork.IssuePriority.Create(priority);
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
                        IssuePriority? existingPriority = await unitOfWork.IssuePriority.GetItemByIdAsync(priority.Id, ct: ct);
                        if (existingPriority == null)
                            unitOfWork.IssuePriority.Create(priority);
                        else
                            existingPriority.CopyData(priority);

                        await unitOfWork.SaveChangesAsync(ct);
                    }, ct);
                }
            }

            logger.LogInformation("[Method:{MethodName}] Update issue priorities completed.", nameof(UpdateIssuePrioritiesFromCloudDb));
        }
    }
}