using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class KindParameterService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IUnitOfWork unitOfWork, IPostgresSelect postgresSelect, EntitySyncService sync, ILogger<KindParameterService> logger)
    {
        public async Task<List<KindsParameter>> GetKindParametersFromCloudApi(CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/parameters?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetRangeOfItemsAsync<KindsParameter>(link, ct: ct);
        }

        private async Task<List<KindsParameter>> GetKindParametersFromCloudDb(CancellationToken ct)
        {
            string sqlCommand = "SELECT id, code, name, field_type FROM equipment_parameters ORDER BY id;";

            DataSet ds = await postgresSelect.Select(sqlCommand, ct);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return new();

            return table.AsEnumerable().
                Select(priority => new KindsParameter
                {
                    Id = priority.Field<int>("id"),
                    Code = priority.Field<string>("code") ?? "",
                    Name = priority.Field<string>("name"),
                    FieldType = priority.Field<int>("field_type").ToString()
                }).ToList();
        }

        public async Task UpdateKindParametersFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind parameters from API.", nameof(UpdateKindParametersFromCloudApi));

            List<KindsParameter> parameters = await GetKindParametersFromCloudApi(ct);

            if (parameters.Count == 0)
                return;

            foreach (KindsParameter item in parameters)
            {
                await sync.RunExclusive(item, async () =>
                {
                    KindsParameter? existingTypes = await unitOfWork.KindParameter.GetItemByIdAsync(item.Id, ct: ct);
                    if (existingTypes == null)
                        unitOfWork.KindParameter.Create(item);
                    else
                        existingTypes.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update kind parameters completed.", nameof(UpdateKindParametersFromCloudApi));
        }

        public async Task UpdateKindParametersFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind parameters from DB.", nameof(UpdateKindParametersFromCloudDb));

            List<KindsParameter> parameters = await GetKindParametersFromCloudDb(ct);

            if (parameters.Count == 0)
                return;

            foreach (KindsParameter item in parameters)
            {
                await sync.RunExclusive(item, async () =>
                {
                    KindsParameter? existingTypes = await unitOfWork.KindParameter.GetItemByIdAsync(item.Id, ct: ct);
                    if (existingTypes == null)
                        unitOfWork.KindParameter.Create(item);
                    else
                        existingTypes.CopyData(item);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update kind parameters completed.", nameof(UpdateKindParametersFromCloudDb));
        }
    }
}