using CRMService.Abstractions.Database.Repository;
using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Models.ConfigClass;
using CRMService.Models.Constants;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;

namespace CRMService.Service.OkdeskEntity
{
    public class MaintenanceEntityService(IOptions<ApiEndpointOptions> endpoint,
        IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILogger<MaintenanceEntityService> logger)
    {
        public async Task<MaintenanceEntity?> GetMaintenanceEntityFromCloudApi(int maintenanceEntityId, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/{maintenanceEntityId}?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetItem<MaintenanceEntity>(link, ct);
        }

        private async IAsyncEnumerable<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/list?api_token={okdeskSettings.Value.OkdeskApiToken}";
            await foreach (List<MaintenanceEntity> me in request.GetAllItems<MaintenanceEntity>(link, startIndex: 0, limit, ct: ct))
                yield return me;
        }

        private async Task<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudDb(long limit, CancellationToken ct)
        {
            string sqlCommand = string.Format(
                "SELECT company_maintenance_entities.sequential_id, company_maintenance_entities.name, companies.sequential_id AS companyId, company_maintenance_entities.active " +
                "FROM company_maintenance_entities " +
                "JOIN companies ON company_maintenance_entities.company_id = companies.id " +
                "ORDER BY company_maintenance_entities.sequential_id  LIMIT '{1}';", limit);

            DataSet ds = await pGSelect.Select(sqlCommand, ct);
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
            MaintenanceEntity? newMaintenanceEntity = await GetMaintenanceEntityFromCloudApi(maintenanceEntityId, ct);

            if (newMaintenanceEntity == null)
                return;

            MaintenanceEntity? existingMaintenance = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(newMaintenanceEntity.Id, ct: ct);
            if (existingMaintenance == null)
                unitOfWork.MaintenanceEntity.Create(newMaintenanceEntity);
            else
                existingMaintenance.CopyData(newMaintenanceEntity);

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateMaintenanceEntitiesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update maintenance entities from API.", nameof(UpdateMaintenanceEntitiesFromCloudApi));

            await foreach (List<MaintenanceEntity> me in GetMaintenanceEntitiesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (MaintenanceEntity newMaintenanceEntity in me)
                {
                    MaintenanceEntity? existingMaintenance = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(newMaintenanceEntity.Id, ct: ct);
                    if (existingMaintenance == null)
                        unitOfWork.MaintenanceEntity.Create(newMaintenanceEntity);
                    else
                        existingMaintenance.CopyData(newMaintenanceEntity);
                }
            }

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateMaintenanceEntitiesFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting updating maintenance entities.", nameof(UpdateMaintenanceEntitiesFromCloudDb));

            while (true)
            {
                List<MaintenanceEntity> me = await GetMaintenanceEntitiesFromCloudDb(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, ct);

                if (me.Count == 0)
                    break;

                foreach (MaintenanceEntity newMaintenanceEntity in me)
                {
                    MaintenanceEntity? existingMaintenance = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(newMaintenanceEntity.Id, ct: ct);
                    if (existingMaintenance == null)
                        unitOfWork.MaintenanceEntity.Create(newMaintenanceEntity);
                    else
                        existingMaintenance.CopyData(newMaintenanceEntity);
                }
            }

            await unitOfWork.SaveAsync(ct);

            logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudDb));
        }
    }
}
