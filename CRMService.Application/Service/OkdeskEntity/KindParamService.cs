using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity
{
    public class KindParamService(IUnitOfWork unitOfWork, IOkdeskUnitOfWork okdeskUnitOfWork, ILogger<KindParamService> logger)
    {
        private async Task<List<KindParam>> GetConnectionsFromCloudDb(CancellationToken ct)
        {
            List<KindParam> parameters = await okdeskUnitOfWork.KindParams.GetItemsByPredicateAsync(asNoTracking: true, ct: ct);

            return parameters.OrderBy(x => x.KindId).ThenBy(x => x.KindParameterId).ToList();
        }

        public async Task UpsertConnectionsFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update kind-parameter connections from DB.", nameof(UpsertConnectionsFromCloudDb));

            List<KindParam> connections = await GetConnectionsFromCloudDb(ct);

            if (connections.Count == 0)
                return;

            HashSet<int> kindIds = new (connections.Select(c => c.KindId));
            HashSet<int> kindParameterIds = new(connections.Select(c => c.KindParameterId));

            await ValidateConnectionReferences(kindIds, kindParameterIds, ct);

            List<KindParam> existingLinks = await unitOfWork.KindParams.GetItemsByPredicateAsync(kp => kindIds.Contains(kp.KindId), asNoTracking: true, ct: ct);

            List<KindParam> toAdd = connections.Except(existingLinks, KindParam.Comparer).ToList();
            List<KindParam> toDelete = existingLinks.Except(connections, KindParam.Comparer).ToList();

            if (toAdd.Count == 0 && toDelete.Count == 0)
                return;

            unitOfWork.KindParams.CreateRange(toAdd);
            unitOfWork.KindParams.DeleteRange(toDelete);

            await unitOfWork.SaveChangesAsync(ct);
        }

        private async Task ValidateConnectionReferences(HashSet<int> kindIds, HashSet<int> kindParameterIds, CancellationToken ct)
        {
            List<Kind> existingKinds = await unitOfWork.Kind.GetItemsByPredicateAsync(
                kind => kindIds.Contains(kind.Id),
                asNoTracking: true,
                ct: ct);

            List<KindsParameter> existingKindParameters = await unitOfWork.KindParameter.GetItemsByPredicateAsync(
                parameter => kindParameterIds.Contains(parameter.Id),
                asNoTracking: true,
                ct: ct);

            List<int> missingKindIds = kindIds.Except(existingKinds.Select(kind => kind.Id)).OrderBy(id => id).ToList();
            List<int> missingKindParameterIds = kindParameterIds.Except(existingKindParameters.Select(parameter => parameter.Id)).OrderBy(id => id).ToList();

            if (missingKindIds.Count == 0 && missingKindParameterIds.Count == 0)
                return;

            string missingKindIdsText = string.Join(", ", missingKindIds);
            string missingKindParameterIdsText = string.Join(", ", missingKindParameterIds);

            logger.LogError("[Method:{MethodName}] Cannot update kind-parameter connections because referenced entities are missing. Missing kind IDs: [{MissingKindIds}]. Missing kind parameter IDs: [{MissingKindParameterIds}].",
                nameof(UpsertConnectionsFromCloudDb), missingKindIdsText, missingKindParameterIdsText);

            throw new InvalidOperationException(
                $"Cannot update kind-parameter connections because referenced entities are missing. Missing kind IDs: [{missingKindIdsText}]. Missing kind parameter IDs: [{missingKindParameterIdsText}].");
        }
    }
}
