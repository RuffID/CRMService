using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class IssuePriorityService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<IssuePriorityService> _logger = logger.CreateLogger<IssuePriorityService>();

        public async Task<List<IssuePriority>> GetIssuePrioritiesFromCloudApi()
        {
            string link = endpoint.Value.OkdeskApi + "/issues/priorities?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItems<IssuePriority>(link);
        }

        public async Task<List<IssuePriority>> GetIssuePrioritiesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM issue_priorities ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
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
            List<IssuePriority> priorities = await GetIssuePrioritiesFromCloudApi();

            if (priorities.Count == 0)
                return;

            for (int i = 1; i < priorities.Count; i++)
                priorities[i - 1].Id = i;

            await unitOfWork.IssuePriority.Upsert(priorities, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateIssuePrioritiesFromCloudDb(CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating issue priorities.", nameof(UpdateIssuePrioritiesFromCloudDb));

            List<IssuePriority> priorities = await GetIssuePrioritiesFromCloudDb();

            if (priorities.Count != 0)
            {
                await unitOfWork.IssuePriority.Upsert(priorities, ct);

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Issue priorities update completed.", nameof(UpdateIssuePrioritiesFromCloudDb));
        }
    }
}