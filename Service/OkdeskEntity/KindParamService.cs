using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.OkdeskEntity;
using System.Data;
using System.Linq;

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

            // Отфильтровать мусор
            List<KindParam> desired = connections
                .Where(c => c.KindId > 0 && c.KindParameterId > 0)
                .Distinct(KindParam.Comparer) // убрать дубли по ключам
                .ToList();

            if (desired.Count == 0)
                return;

            // Собрать Id всех Kind для ограничения области
            HashSet<int> kindIds = new(desired.Select(c => c.KindId));

            List<KindParam> existingLinks = await unitOfWork.KindParams.GetItemsByPredicate(kp => kindIds.Contains(kp.KindId), asNoTracking: true, ct: ct);

            // Вычислить дельту через компаратор
            List<KindParam> toAdd = desired.Except(existingLinks, KindParam.Comparer).ToList();
            List<KindParam> toDelete = existingLinks.Except(desired, KindParam.Comparer).ToList();

            if (toAdd.Count == 0 && toDelete.Count == 0)
                return;

            // Применить дельту одной транзакцией
            await unitOfWork.ExecuteInTransaction(async () =>
            {
                // Добавить недостающие связи
                foreach (KindParam link in toAdd)
                    unitOfWork.KindParams.Create(link); // Добавить связь kind-param

                // Удалить неактуальные связи
                foreach (KindParam link in toDelete)
                    unitOfWork.KindParams.Delete(link); // Удалить связь kind-param

                await unitOfWork.SaveAsync(ct); // Зафиксировать пакет изменений
            }, ct);
        }
    }
}