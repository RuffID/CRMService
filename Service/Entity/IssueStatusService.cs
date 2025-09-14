using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class IssueStatusService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect)
    {
        public async Task<List<IssueStatus>> GetIssueStatusesFromCloudApi()
        {
            string link = endpoint.Value.OkdeskApi + "/issues/statuses?api_token=" + okdeskSettings.Value.OkdeskApiToken;

            return await request.GetRangeOfItems<IssueStatus>(link);
        }

        public async Task<List<IssueStatus>> GetIssueStatusesFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM issue_statuses ORDER BY id;;";

            DataSet ds = await pGSelect.Select(sqlCommand);
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
            List<IssueStatus> statuses = await GetIssueStatusesFromCloudApi();

            if (statuses.Count == 0)
                return;

            for (int i = 1; i < statuses.Count; i++)
                statuses[i-1].Id = i;

            await unitOfWork.IssueStatus.Upsert(statuses, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateIssueStatusesFromCloudDb(CancellationToken ct)
        {
            List<IssueStatus> statuses = await GetIssueStatusesFromCloudDb();

            if (statuses.Count == 0)
                return;

            await unitOfWork.IssueStatus.Upsert(statuses, ct);

            await unitOfWork.SaveAsync(ct);

        }
    }
}