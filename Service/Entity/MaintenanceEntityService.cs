using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class MaintenanceEntityService(IOptions<ApiEndpointOptions> endpoint, 
        IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<MaintenanceEntityService> _logger = logger.CreateLogger<MaintenanceEntityService>();

        public async Task<MaintenanceEntity?> GetMaintenanceEntityFromCloudApi(int maintenanceEntityId)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/{maintenanceEntityId}?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetItem<MaintenanceEntity>(link);
        }

        private async IAsyncEnumerable<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/list?api_token={okdeskSettings.Value.OkdeskApiToken}";
            await foreach (List<MaintenanceEntity> me in request.GetAllItems<MaintenanceEntity>(link, startIndex, limit))
                yield return me;
        }

        private async Task<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudDb(long startIndex, long limit)
        {
            string sqlCommand = string.Format(
                "SELECT company_maintenance_entities.sequential_id, company_maintenance_entities.name, companies.sequential_id AS companyId, company_maintenance_entities.active " +
                "FROM company_maintenance_entities " +
                "JOIN companies ON company_maintenance_entities.company_id = companies.id " +
                "AND company_maintenance_entities.sequential_id > '{0}' ORDER BY company_maintenance_entities.sequential_id  LIMIT '{1}';", startIndex, limit);

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? meTable = ds.Tables["Table"];
            if (meTable == null)
                return new();

            return meTable.AsEnumerable().
                Select(me => new MaintenanceEntity
                {
                    Id = me.Field<int>("sequential_id"),
                    Name = me.Field<string>("name") ?? string.Empty,
                    CompanyId = me.Field<int>("companyId"),
                    Active = me.Field<bool>("active")
                }).ToList();
        }

        public async Task UpdateMaintenanceEntityFromCloudApi(int maintenanceEntityId, CancellationToken ct)
        {
            MaintenanceEntity? newMaintenanceEntity = await GetMaintenanceEntityFromCloudApi(maintenanceEntityId);

            if (newMaintenanceEntity == null)
                return;

            await unitOfWork.MaintenanceEntity.Upsert(newMaintenanceEntity, ct);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateMaintenanceEntitiesFromCloudApi(long startIndex, long limit, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating maintenance entities.", nameof(UpdateMaintenanceEntitiesFromCloudApi));

            await foreach (List<MaintenanceEntity> me in GetMaintenanceEntitiesFromCloudApi(startIndex, limit))
            {
                if (me.Count == 0)
                    continue;

                await unitOfWork.MaintenanceEntity.Upsert(me, ct);

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudApi));
        }

        public async Task UpdateMaintenanceEntitiesFromCloudDb(long startIndex, long limit, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating maintenance entities.", nameof(UpdateMaintenanceEntitiesFromCloudDb));

            while (true)
            {
                List<MaintenanceEntity> me = await GetMaintenanceEntitiesFromCloudDb(startIndex, limit);

                if (me.Count == 0)
                    break;

                startIndex = me.Last().Id;

                await unitOfWork.MaintenanceEntity.Upsert(me, ct);

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudDb));
        }
    }
}
