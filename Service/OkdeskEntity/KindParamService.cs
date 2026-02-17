using CRMService.Abstractions.Database.Repository;
using CRMService.DataBase.Postgresql;
using CRMService.Models.OkdeskEntity;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class KindParamService(IUnitOfWork unitOfWork, PGSelect pGSelect, ILogger<KindParamService> logger)
    {
        private async Task<List<KindParam>> GetConnectionsFromCloudDb(CancellationToken ct)
        {
            const string SQL = "SELECT equipment_kind_id, parameter_id FROM equipment_kind_parameters;";

            DataSet ds = await pGSelect.Select(SQL, ct);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return new();

            return table.AsEnumerable().
                Select(group => new KindParam
                {
                    KindId = group.Field<int>("equipment_kind_id"),
                    KindParameterId = group.Field<int>("parameter_id")
                }).ToList();
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