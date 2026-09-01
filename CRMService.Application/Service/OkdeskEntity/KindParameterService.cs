using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.ConfigClass;
using CRMService.Application.Models.OkdeskApi;
using CRMService.Application.Models.OkdeskSource;
using CRMService.Application.Service.Sync;
using CRMService.Domain.Models.OkdeskEntity;
using EFCoreLibrary.Abstractions.Entity;
using Microsoft.Extensions.Options;
using CRMService.Application.Abstractions.Service;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class KindParameterService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, IOkdeskEntityRequestService request, IEquipmentUnitOfWork unitOfWork, IOkdeskEquipmentSource okdeskUnitOfWork, EntitySyncService sync, ILogger<KindParameterService> logger)
    {
        public Task<List<KindsParameter>> GetKindParametersAsync(CancellationToken ct = default) =>
            unitOfWork.KindParameter.GetItemsReadOnlyAsync(ct);

        public async Task<List<EquipmentParameterSchema>> GetKindParametersFromCloudApi(CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/parameters?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetRangeOfItemsAsync<EquipmentParameterSchema>(link, ct: ct);
        }

        private async Task<List<OkdeskKindParameterRecord>> GetKindParametersFromCloudDb(CancellationToken ct)
        {
            List<OkdeskKindParameterRecord> parameters = await okdeskUnitOfWork.KindParameter.GetAllReadOnlyAsync(ct);

            return parameters.OrderBy(x => x.Id).ToList();
        }


        public async Task UpdateKindParametersFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind parameters from API.", nameof(UpdateKindParametersFromCloudApi));

            List<EquipmentParameterSchema> parameters = await GetKindParametersFromCloudApi(ct);

            foreach (EquipmentParameterSchema item in parameters)
            {
                ValidateCode(item.Code);

                await sync.RunExclusive(new KindParameterSyncKey(item.Code), async () =>
                {
                    KindsParameter? existingParameter = await unitOfWork.KindParameter.GetByCodeAsync(item.Code, ct);

                    if (existingParameter == null)
                        unitOfWork.KindParameter.Create(new KindsParameter(item.Code, item.Name, item.FieldType));
                    else
                        existingParameter.UpdateDetails(item.Code, item.Name, item.FieldType);

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update kind parameters completed.", nameof(UpdateKindParametersFromCloudApi));
        }

        public async Task UpdateKindParametersFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind parameters from DB.", nameof(UpdateKindParametersFromCloudDb));

            List<OkdeskKindParameterRecord> parameters = await GetKindParametersFromCloudDb(ct);

            foreach (OkdeskKindParameterRecord item in parameters)
            {
                ValidateCode(item.Code);

                await sync.RunExclusive(new KindParameterSyncKey(item.Code), async () =>
                {
                    KindsParameter? parameterByOkdeskId = await unitOfWork.KindParameter.GetByOkdeskIdAsync(item.Id, ct);
                    KindsParameter? parameterByCode = await unitOfWork.KindParameter.GetByCodeAsync(item.Code, ct);

                    if (parameterByOkdeskId != null && parameterByCode != null && parameterByOkdeskId.Id != parameterByCode.Id)
                    {
                        throw new InvalidOperationException(
                            $"Kind parameter conflict: Okdesk id '{item.Id}' belongs to local parameter '{parameterByOkdeskId.Id}', " +
                            $"but code '{item.Code}' belongs to local parameter '{parameterByCode.Id}'.");
                    }

                    KindsParameter? existingParameter = parameterByOkdeskId ?? parameterByCode;
                    if (existingParameter == null)
                    {
                        unitOfWork.KindParameter.Create(new KindsParameter(item.Code, item.Name, item.FieldType, item.Id));
                    }
                    else
                    {
                        existingParameter.SetOkdeskId(item.Id);
                        existingParameter.UpdateDetails(item.Code, item.Name, item.FieldType);
                    }

                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }

            logger.LogInformation("[Method:{MethodName}] Update kind parameters completed.", nameof(UpdateKindParametersFromCloudDb));
        }

        private static void ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Kind parameter source returned an empty code.");
        }

        private sealed class KindParameterSyncKey(string code) : IEntity<string>
        {
            public string Id { get; set; } = code;
        }
    }
}
