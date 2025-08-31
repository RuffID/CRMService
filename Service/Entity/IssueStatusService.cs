using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class IssueStatusService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetItemService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<IssueStatusService> _logger = logger.CreateLogger<IssueStatusService>();

        public async Task<List<IssueStatus>?> GetIssueStatusesFromCloudApi()
        {
            string link = endpoint.Value.OkdeskApi + "/issues/statuses?api_token=" + okdeskSettings.Value.ApiToken;

            return await request.GetRangeOfItems<IssueStatus>(link);
        }

        public async Task<List<IssueStatus>?> GetIssueStatusesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM issue_statuses ORDER BY id;;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(status => new IssueStatus
                {
                    Code = status.Field<string>("code") ?? "",
                    Name = status.Field<string>("name")
                }).ToList();
        }

        public async Task UpdateIssueStatusesFromCloudApi(CancellationToken ct)
        {
            List<IssueStatus>? statuses = await GetIssueStatusesFromCloudApi();

            if (statuses == null || statuses.Count == 0)
                return;

            await unitOfWork.IssueStatus.UpsertByCodes(statuses, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating issue statuses.", nameof(UpdateIssueStatusesFromCloudDb));

            List<IssueStatus>? statuses = await GetIssueStatusesFromCloudDb();

            if (statuses == null || statuses.Count == 0)
                return;

            await unitOfWork.IssueStatus.UpsertByCodes(statuses, ct);

            await unitOfWork.SaveAsync(ct);

            _logger.LogInformation("[Method:{MethodName}] Issue statuses update completed.", nameof(UpdateIssueStatusesFromCloudDb));
        }
    }
}