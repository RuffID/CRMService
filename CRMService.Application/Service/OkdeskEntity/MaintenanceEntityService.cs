using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Domain.Models.Constants;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class MaintenanceEntityService(IOptions<ApiEndpointOptions> endpoint,
        IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, postgresSelect postgresSelect, ILogger<MaintenanceEntityService> logger)
    {
        public async Task<MaintenanceEntity?> GetMaintenanceEntityFromCloudApi(int maintenanceEntityId, CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/{maintenanceEntityId}?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetItemAsync<MaintenanceEntity>(link, ct);
        }

        private async IAsyncEnumerable<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/maintenance_entities/list?api_token={okdeskSettings.Value.OkdeskApiToken}";
            await foreach (List<MaintenanceEntity> me in request.GetAllItemsAsync<MaintenanceEntity>(link, startIndex: 0, limit, ct: ct))
                yield return me;
        }

        private async Task<List<MaintenanceEntity>> GetMaintenanceEntitiesFromCloudDb(long limit, CancellationToken ct)
        {
            string sqlCommand = string.Format(
                "SELECT company_maintenance_entities.sequential_id, company_maintenance_entities.name, companies.sequential_id AS companyId, company_maintenance_entities.active " +
                "FROM company_maintenance_entities " +
                "JOIN companies ON company_maintenance_entities.company_id = companies.id " +
                "ORDER BY company_maintenance_entities.sequential_id  LIMIT '{1}';", limit);

            DataSet ds = await postgresSelect.Select(sqlCommand, ct);
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

            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task UpdateMaintenanceEntitiesFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update maintenance entities from API.", nameof(UpdateMaintenanceEntitiesFromCloudApi));

            await foreach (List<MaintenanceEntity> me in GetMaintenanceEntitiesFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (MaintenanceEntity newMaintenanceEntity in me)
                {
                    await CheckMaintenanceEntity(newMaintenanceEntity, ct);

                    MaintenanceEntity? existingMaintenance = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(newMaintenanceEntity.Id, ct: ct);
                    if (existingMaintenance == null)
                        unitOfWork.MaintenanceEntity.Create(newMaintenanceEntity);
                    else
                        existingMaintenance.CopyData(newMaintenanceEntity);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
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
                    await CheckMaintenanceEntity(newMaintenanceEntity, ct);

                    MaintenanceEntity? existingMaintenance = await unitOfWork.MaintenanceEntity.GetItemByIdAsync(newMaintenanceEntity.Id, ct: ct);
                    if (existingMaintenance == null)
                        unitOfWork.MaintenanceEntity.Create(newMaintenanceEntity);
                    else
                        existingMaintenance.CopyData(newMaintenanceEntity);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("[Method:{MethodName}] Maintenance entities update completed.", nameof(UpdateMaintenanceEntitiesFromCloudDb));
        }

        private async Task CheckMaintenanceEntity(MaintenanceEntity maintenanceEntity, CancellationToken ct)
        {
            if (maintenanceEntity.Company != null)
                maintenanceEntity.CompanyId = (await unitOfWork.Company.GetItemByPredicateAsync(c => c.Id == maintenanceEntity.Company.Id, asNoTracking: true, ct: ct))?.Id;            
            else if (maintenanceEntity.CompanyId.HasValue)
                maintenanceEntity.CompanyId = (await unitOfWork.Company.GetItemByPredicateAsync(c => c.Id == maintenanceEntity.CompanyId, asNoTracking: true, ct: ct))?.Id;

            if (maintenanceEntity.Company != null)
            {
                Company? company = await unitOfWork.Company.GetItemByIdAsync(maintenanceEntity.Company.Id, true, ct: ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for maintenanceEntity with id: {maintenanceEntityId}.",
                        nameof(CheckMaintenanceEntity), maintenanceEntity.Company.Id, maintenanceEntity.Id);
                    maintenanceEntity.CompanyId = null;
                    maintenanceEntity.Company = null;
                }
                else
                {
                    maintenanceEntity.CompanyId = company?.Id;
                }
            }
            else if (maintenanceEntity.CompanyId.HasValue)
            {
                Company? company = await unitOfWork.Company.GetItemByIdAsync(maintenanceEntity.CompanyId.Value, true, ct: ct);
                if (company == null)
                {
                    logger.LogWarning("[Method:{MethodName}] Company with id: {CompanyId} was not found for maintenanceEntity with id: {maintenanceEntityId}.",
                        nameof(CheckMaintenanceEntity), maintenanceEntity.CompanyId, maintenanceEntity.Id);
                    maintenanceEntity.CompanyId = null;
                }
                else
                {
                    maintenanceEntity.CompanyId = company?.Id;
                }
            }
        }
    }
}