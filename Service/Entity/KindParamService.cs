using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.Entity;
using System.Data;

namespace CRMService.Service.Entity
{
    public class KindParamService(IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<KindParamService> _logger = logger.CreateLogger<KindParamService>();

        private async Task<List<(string? kindParameterCode, string? kindCode)>?> GetConnectionsFromCloudDb()
        {
            string sqlCommand = "SELECT equipment_kind_parameters.id, equipment_parameters.code AS kindParameterCode, equipment_kinds.code AS kindCode " +
                    "FROM equipment_kinds " +
                    "LEFT OUTER JOIN equipment_kind_parameters ON equipment_kinds.id = equipment_kind_parameters.equipment_kind_id " +
                    "LEFT OUTER JOIN equipment_parameters ON equipment_kind_parameters.parameter_id = equipment_parameters.id " +
                    "ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(x => (x.Field<string?>("kindParameterCode"), x.Field<string?>("kindCode"))).ToList();
        }

        public async Task UpdateConnectionsFromCloudDb(CancellationToken ct)
        {
            //TODO проверить как отрабатывает. Для kindParam элементы были null(?), сделал не nullable
            _logger.LogInformation("[Method:{MethodName}] Starting updating kind - parameter connections.", nameof(UpdateConnectionsFromCloudDb));

            List<(string? kindParameterCode, string? kindCode)>? connections = await GetConnectionsFromCloudDb();
            List<KindParam> kindParams = [];

            if (connections == null || connections.Count == 0)
                return;

            foreach (var (kindParameterCode, kindCode) in connections)
            {
                if (string.IsNullOrWhiteSpace(kindParameterCode) || string.IsNullOrEmpty(kindCode)) continue;

                KindsParameter? parameter = await unitOfWork.KindParameter.GetItemByPredicate(kp => kp.Code == kindParameterCode, asNoTracking: true, ct);
                Kind? kind = await unitOfWork.Kind.GetItemByPredicate(k => k.Code == kindCode, asNoTracking: true, ct);

                if (parameter == null || kind == null) 
                    continue;

                KindParam connection = new() { KindParameterId = parameter.Id, KindId = kind.Id };

                KindParam? connectionFromLocalDb = await unitOfWork.KindParams.GetItemByPredicate(predicate: kp => kp.KindParameterId == parameter.Id && kp.KindId == kind.Id, asNoTracking: true, ct: ct);

                if (connectionFromLocalDb == null)
                    unitOfWork.KindParams.Create(connection);

                kindParams.Add(connection);
            }

            await unitOfWork.SaveAsync(ct);

            await DeleteIrrelevantConnectionsBetweenKindAndKindParameterFromCloudDb(kindParams, ct);

            _logger.LogInformation("[Method:{MethodName}] Kind - parameter connections update completed.", nameof(UpdateConnectionsFromCloudDb));
        }

        private async Task DeleteIrrelevantConnectionsBetweenKindAndKindParameterFromCloudDb(List<KindParam> kindParamsFromCloudDb, CancellationToken ct)
        {
            foreach (KindParam connectionFromCloudDb in kindParamsFromCloudDb)
            {
                List<KindParam> localConnections = await unitOfWork.KindParams.GetItemsByPredicate(predicate: kp => kp.KindParameterId == connectionFromCloudDb.KindParameterId && kp.KindId == connectionFromCloudDb.KindId, asNoTracking: true, ct: ct);

                if (localConnections.Count == 0)
                    continue;

                foreach (KindParam connection in localConnections)
                {
                    // Если связи нет в облачном API, но она есть в локальной БД
                    if (kindParamsFromCloudDb.FirstOrDefault(c => c.KindId == connection.KindId && c.KindParameterId == connection.KindParameterId) == null)
                        // Удалить такую связь т.к. она больше неактуальна
                        unitOfWork.KindParams.Delete(connection);
                }
            }

            await unitOfWork.SaveAsync(ct);
        }
    }
}