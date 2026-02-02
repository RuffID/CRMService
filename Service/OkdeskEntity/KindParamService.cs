using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.OkdeskEntity;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class KindParamService(IUnitOfWork unitOfWork, PGSelect pGSelect)
    {
        private async Task<List<KindParam>> GetConnectionsFromCloudDb()
        {
            /* string sqlCommand = "SELECT equipment_kind_parameters.id, equipment_parameters.code AS kindParameterCode, equipment_kinds.code AS kindCode " +
                     "FROM equipment_kinds " +
                     "LEFT OUTER JOIN equipment_kind_parameters ON equipment_kinds.id = equipment_kind_parameters.equipment_kind_id " +
                     "LEFT OUTER JOIN equipment_parameters ON equipment_kind_parameters.parameter_id = equipment_parameters.id " +
                     "ORDER BY id;";*/

            const string SQL = "SELECT equipment_kind_id, parameter_id FROM equipment_kind_parameters;";

            DataSet ds = await pGSelect.Select(SQL);
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
            List<KindParam> connections = await GetConnectionsFromCloudDb();

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

            await unitOfWork.SaveAsync(ct);
        }
    }
}