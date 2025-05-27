using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class KindParameterService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<KindParameterService> _logger = logger.CreateLogger<KindParameterService>();

        public async Task<List<KindsParameter>?> GetKindParametersFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi} /equipments/parameters?api_token= {okdeskSettings.Value.ApiToken}";

            return await request.GetRangeOfItems<KindsParameter>(link);
        }

        private async Task<List<KindsParameter>?> GetKindParametersFromCloudDb()
        {
            string sqlCommand = "SELECT sequential_id AS id, code, name FROM equipment_parameters ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(priority => new KindsParameter
                {
                    Code = priority.Field<string>("code"),
                    Name = priority.Field<string>("name")
                }).ToList();
        }

        public async Task UpdateKindParametersFromCloudApi()
        {
            List<KindsParameter>? parameters = await GetKindParametersFromCloudApi();

            if (parameters == null || parameters.Count == 0) return;

            await unitOfWork.KindParameter.CreateOrUpdate(parameters);

            await unitOfWork.SaveAsync();

            await UpdateConnectionsFromCloudApi(parameters);
        }

        public async Task UpdateKindParametersFromCloudDb()
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating kind parameters.", nameof(UpdateKindParametersFromCloudDb));

            List<KindsParameter>? kinds = await GetKindParametersFromCloudDb();

            if (kinds == null || kinds.Count == 0)
                return;

            await unitOfWork.KindParameter.CreateOrUpdate(kinds);

            await unitOfWork.SaveAsync();

            _logger.LogInformation("[Method:{MethodName}] Kind parameters update completed.", nameof(UpdateKindParametersFromCloudDb));
        }

        public async Task UpdateConnectionsFromCloudApi(List<KindsParameter> parameters)
        {
            foreach (KindsParameter parameter in parameters)
            {
                if (parameter.Equipment_kind_codes == null || parameter.Equipment_kind_codes.Length == 0)
                    continue;

                foreach (string code in parameter.Equipment_kind_codes)
                {
                    KindsParameter? paramFromLocalDb = await unitOfWork.KindParameter.GetItem(parameter, false);
                    Kind? kindFromLocalDb = await unitOfWork.Kind.GetKindByCode(code, false);

                    if (paramFromLocalDb == null || kindFromLocalDb == null)
                        continue;

                    KindParam connection = new() { KindId = kindFromLocalDb.Id, KindParameterId = paramFromLocalDb.Id };
                    if (await unitOfWork.KindParams.GetItem(connection, false) == null)
                        unitOfWork.KindParams.Create(connection);
                }
            }
            await unitOfWork.SaveAsync();
        }
    }
}