using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class KindParameterService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<KindParameterService> _logger = logger.CreateLogger<KindParameterService>();

        public async Task<List<KindsParameter>> GetKindParametersFromCloudApi()
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/parameters?api_token={okdeskSettings.Value.OkdeskApiToken}";

            return await request.GetRangeOfItems<KindsParameter>(link);
        }

        private async Task<List<KindsParameter>> GetKindParametersFromCloudDb()
        {
            string sqlCommand = "SELECT id, code, name, field_type FROM equipment_parameters ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
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
            List<KindsParameter> parameters = await GetKindParametersFromCloudApi();

            if (parameters.Count == 0)
                return;

            foreach (KindsParameter item in parameters)
            {
                KindsParameter? existingTypes = await unitOfWork.KindParameter.GetItemByIdAsync(item.Id, ct: ct);
                if (existingTypes == null)
                    unitOfWork.KindParameter.Create(item);
                else
                    existingTypes.CopyData(item);
            }

            await unitOfWork.SaveAsync(ct);
        }

        public async Task UpdateKindParametersFromCloudDb(CancellationToken ct)
        {
            List<KindsParameter> parameters = await GetKindParametersFromCloudDb();

            if (parameters.Count == 0)
                return;

            foreach (KindsParameter item in parameters)
            {
                KindsParameter? existingTypes = await unitOfWork.KindParameter.GetItemByIdAsync(item.Id, ct: ct);
                if (existingTypes == null)
                    unitOfWork.KindParameter.Create(item);
                else
                    existingTypes.CopyData(item);
            }

            await unitOfWork.SaveAsync(ct);
        }
    }
}