using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Models.OkdeskSource;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class KindParamService(IEquipmentUnitOfWork unitOfWork, IOkdeskEquipmentSource okdeskUnitOfWork, ILogger<KindParamService> logger)
    {
        private async Task<List<OkdeskKindParameterConnectionRecord>> GetConnectionsFromCloudDb(CancellationToken ct)
        {
            List<OkdeskKindParameterConnectionRecord> parameters = await okdeskUnitOfWork.KindParams.GetAllReadOnlyAsync(ct);

            return parameters.OrderBy(x => x.KindId).ThenBy(x => x.KindParameterId).ToList();
        }

        public async Task UpsertConnectionsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind-parameter connections from DB.", nameof(UpsertConnectionsFromCloudDb));

            List<OkdeskKindParameterConnectionRecord> sourceConnections = await GetConnectionsFromCloudDb(ct);

            if (sourceConnections.Count == 0)
                return;

            HashSet<int> kindIds = new(sourceConnections.Select(c => c.KindId));
            HashSet<int> okdeskKindParameterIds = new(sourceConnections.Select(c => c.KindParameterId));

            Dictionary<int, int> localParameterIdsByOkdeskId = await ResolveLocalParameterIds(okdeskKindParameterIds, ct);
            await ValidateKindReferences(kindIds, ct);

            List<KindParam> connections = sourceConnections
                .Select(connection => new KindParam
                {
                    KindId = connection.KindId,
                    KindParameterId = localParameterIdsByOkdeskId[connection.KindParameterId]
                })
                .Distinct(KindParam.Comparer)
                .ToList();

            List<KindParam> existingLinks = await unitOfWork.KindParams.GetByKindIdsReadOnlyAsync(kindIds, ct);

            List<KindParam> toAdd = connections.Except(existingLinks, KindParam.Comparer).ToList();
            List<KindParam> toDelete = existingLinks.Except(connections, KindParam.Comparer).ToList();

            if (toAdd.Count == 0 && toDelete.Count == 0)
                return;

            unitOfWork.KindParams.CreateRange(toAdd);
            unitOfWork.KindParams.DeleteRange(toDelete);

            await unitOfWork.SaveChangesAsync(ct);
        }

        private async Task<Dictionary<int, int>> ResolveLocalParameterIds(HashSet<int> okdeskKindParameterIds, CancellationToken ct)
        {
            List<KindsParameter> existingKindParameters = await unitOfWork.KindParameter.GetByOkdeskIdsReadOnlyAsync(okdeskKindParameterIds, ct);
            Dictionary<int, int> localIdsByOkdeskId = existingKindParameters
                .Where(parameter => parameter.OkdeskId.HasValue)
                .ToDictionary(parameter => parameter.OkdeskId!.Value, parameter => parameter.Id);
            List<int> missingOkdeskIds = okdeskKindParameterIds
                .Except(localIdsByOkdeskId.Keys)
                .OrderBy(id => id)
                .ToList();

            if (missingOkdeskIds.Count == 0)
                return localIdsByOkdeskId;

            string missingOkdeskIdsText = string.Join(", ", missingOkdeskIds);
            logger.LogError("[Method:{MethodName}] Cannot update kind-parameter connections because Okdesk parameters are missing locally. Missing Okdesk parameter IDs: [{MissingOkdeskIds}].",
                nameof(UpsertConnectionsFromCloudDb), missingOkdeskIdsText);

            throw new InvalidOperationException(
                $"Cannot update kind-parameter connections because Okdesk parameters are missing locally. Missing Okdesk parameter IDs: [{missingOkdeskIdsText}].");
        }

        private async Task ValidateKindReferences(HashSet<int> kindIds, CancellationToken ct)
        {
            List<Kind> existingKinds = await unitOfWork.Kind.GetByIdsReadOnlyAsync(kindIds, ct);

            List<int> missingKindIds = kindIds.Except(existingKinds.Select(kind => kind.Id)).OrderBy(id => id).ToList();

            if (missingKindIds.Count == 0)
                return;

            string missingKindIdsText = string.Join(", ", missingKindIds);

            logger.LogError("[Method:{MethodName}] Cannot update kind-parameter connections because referenced equipment kinds are missing. Missing kind IDs: [{MissingKindIds}].",
                nameof(UpsertConnectionsFromCloudDb), missingKindIdsText);

            throw new InvalidOperationException(
                $"Cannot update kind-parameter connections because referenced equipment kinds are missing. Missing kind IDs: [{missingKindIdsText}].");
        }
    }
}
