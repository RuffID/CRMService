using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class IssuePriorityService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<IssuePriorityService> _logger = logger.CreateLogger<IssuePriorityService>();

        public async Task<List<IssuePriority>?> GetIssuePrioritiesFromCloudApi()
        {
            string link = endpoint.Value.OkdeskApi + "/issues/priorities?api_token=" + okdeskSettings.Value.ApiToken;

            return await request.GetRangeOfItems<IssuePriority>(link);
        }

        public async Task<List<IssuePriority>?> GetIssuePrioritiesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM issue_priorities ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(priority => new IssuePriority
                {
                    Code = priority.Field<string>("code"),
                    Name = priority.Field<string>("name")
                }).ToList();
        }

        public async Task UpdateIssuePrioritiesFromCloudApi()
        {
            List<IssuePriority>? priorities = await GetIssuePrioritiesFromCloudApi();

            if (priorities == null || priorities.Count == 0)
                return;

            await unitOfWork.IssuePriority.CreateOrUpdate(priorities);

            await unitOfWork.SaveAsync();
        }

        public async Task UpdateIssuePrioritiesFromCloudDb()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating issue priorities.", nameof(UpdateIssuePrioritiesFromCloudDb));

            List<IssuePriority>? priorities = await GetIssuePrioritiesFromCloudDb();

            if (priorities == null || priorities.Count == 0)
                return;

            await unitOfWork.IssuePriority.CreateOrUpdate(priorities);

            await unitOfWork.SaveAsync();

            _logger.LogInformation("[Method:{MethodName}] Issue priorities update completed.", nameof(UpdateIssuePrioritiesFromCloudDb));
        }
    }
}