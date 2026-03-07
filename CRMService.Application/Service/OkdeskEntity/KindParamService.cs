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

            List<KindParam> existingLinks = await unitOfWork.KindParams.GetItemsByPredicateAsync(kp => kindIds.Contains(kp.KindId), asNoTracking: true, ct: ct);

            List<KindParam> toAdd = connections.Except(existingLinks, KindParam.Comparer).ToList();
            List<KindParam> toDelete = existingLinks.Except(connections, KindParam.Comparer).ToList();

            if (toAdd.Count == 0 && toDelete.Count == 0)
                return;

            unitOfWork.KindParams.CreateRange(toAdd);
            unitOfWork.KindParams.DeleteRange(toDelete);

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}