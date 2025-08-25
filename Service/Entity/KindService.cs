using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class KindService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<KindService> _logger = logger.CreateLogger<KindService>();

        private async IAsyncEnumerable<List<Kind>?> GetKindsFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/kinds?api_token={okdeskSettings.Value.ApiToken}";

            await foreach (List<Kind> kinds in request.GetAllItems<Kind>(link, startIndex, limit))
                yield return kinds;
        }

        private async Task<List<Kind>?> GetKindsFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM equipment_kinds ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(priority => new Kind
                {
                    Code = priority.Field<string>("code"),
                    Name = priority.Field<string>("name")
                }).ToList();
        }

        public async Task UpdateKindsFromCloudApi(long startIndex, long limit)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating kinds.", nameof(UpdateKindsFromCloudApi));

            await foreach (List<Kind>? kinds in GetKindsFromCloudApi(startIndex, limit))
            {
                if (kinds == null || kinds.Count == 0) return;

                await unitOfWork.Kind.CreateOrUpdate(kinds);

                await unitOfWork.SaveAsync();
            }

            _logger.LogInformation("[Method:{MethodName}] Kinds update completed.", nameof(UpdateKindsFromCloudApi));

        }

        public async Task UpdateKindsFromCloudDb()
        {
            List<Kind>? kinds = await GetKindsFromCloudDb();

            if (kinds == null || kinds.Count == 0)
                return;

            await unitOfWork.Kind.CreateOrUpdate(kinds);

            await unitOfWork.SaveAsync();
        }
    }
}