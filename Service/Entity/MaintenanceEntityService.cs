using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using CRMService.Service.Sync;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class MaintenanceEntityService(IOptions<ApiEndpoint> endpoint, EntitySyncService sync, IOptions<DatabaseSettings> dbSettings, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<MaintenanceEntityService> _logger = logger.CreateLogger<MaintenanceEntityService>();

        public async Task<MaintenanceEntity?> GetMaintenanceEntityFromCloudApi(int maintenanceEntityId)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/{maintenanceEntityId}?api_token={okdeskSettings.Value.ApiToken}";

            return await request.GetItem<MaintenanceEntity>(link);
        }

        private async IAsyncEnumerable<List<MaintenanceEntity>?> GetMaintenanceEntitiesFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/list?api_token={okdeskSettings.Value.ApiToken}";
            await foreach (List<MaintenanceEntity> me in request.GetAllItems<MaintenanceEntity>(link, startIndex, limit))
                yield return me;
        }

        private async Task<List<MaintenanceEntity>?> GetMaintenanceEntitiesFromCloudDb(int startIndex, int limit)
        {
            string sqlCommand = string.Format(
                "SELECT company_maintenance_entities.sequential_id, company_maintenance_entities.name, companies.sequential_id AS companyId, company_maintenance_entities.active " +
                "FROM company_maintenance_entities " +
                "JOIN companies ON company_maintenance_entities.company_id = companies.id " +
                "AND company_maintenance_entities.sequential_id > '{0}' ORDER BY company_maintenance_entities.sequential_id  LIMIT '{1}';", startIndex, limit);

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? meTable = ds.Tables["company_maintenance_entities"];
            if (meTable == null)
                return null;

            return meTable.AsEnumerable().
                Select(me => new MaintenanceEntity
                {
                    Id = me.Field<int>("sequential_id"),
                    Name = me.Field<string>("name"),
                    CompanyId = me.Field<int>("companyId"),
                    Active = me.Field<sbyte>("active")
                }).ToList();
        }

        public async Task UpdateMaintenanceEntityFromCloudApi(int maintenanceEntityId)
        {
            MaintenanceEntity? maintenanceEntity = await GetMaintenanceEntityFromCloudApi(maintenanceEntityId);

            if (maintenanceEntity == null)
                return;

            await sync.RunExclusive(async () =>
            {
                if (await unitOfWork.MaintenanceEntity.GetItem(maintenanceEntity, false) == null)
                    unitOfWork.MaintenanceEntity.Create(maintenanceEntity);
                else
                    unitOfWork.MaintenanceEntity.Update(maintenanceEntity);

                await unitOfWork.SaveAsync();
            });
        }

        public async Task UpdateMaintenanceEntitiesFromCloudApi(long startIndex, long limit)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating maintenance entities.", nameof(UpdateMaintenanceEntitiesFromCloudApi));

            await sync.RunExclusive(async () =>
            {
                await foreach (List<MaintenanceEntity>? me in GetMaintenanceEntitiesFromCloudApi(startIndex, limit))
                {
                    if (me == null || me.Count == 0)
                        return;

                    await unitOfWork.MaintenanceEntity.CreateOrUpdate(me);

                    await unitOfWork.SaveAsync();
                }
            });

            _logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudApi));
        }

        public async Task UpdateMaintenanceEntitiesFromCloudDb()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating maintenance entities.", nameof(UpdateMaintenanceEntitiesFromCloudDb));

            int indexOfME = 0;

            await sync.RunExclusive(async () =>
            {
                while (true)
                {
                    List<MaintenanceEntity>? me = await GetMaintenanceEntitiesFromCloudDb(indexOfME, dbSettings.Value.LimitForRetrievingEntitiesFromDb);

                    if (me == null || me.Count == 0)
                        break;

                    indexOfME = me.Last().Id;

                    await unitOfWork.MaintenanceEntity.CreateOrUpdate(me);

                    await unitOfWork.SaveAsync();
                }
            });

            _logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudDb));
        }
    }
}
