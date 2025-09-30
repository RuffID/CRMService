using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.OkdeskEntity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.OkdeskEntity
{
    public class ManufacturerService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<ManufacturerService> _logger = logger.CreateLogger<ManufacturerService>();

        private async IAsyncEnumerable<List<Manufacturer>> GetManufacturersFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/manufacturers?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Manufacturer> manufacturers in request.GetAllItems<Manufacturer>(link, startIndex, limit))
                yield return manufacturers;
        }

        private async Task<List<Manufacturer>> GetManufacturersFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM equipment_manufacturers ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return new();

            return table.AsEnumerable().
                Select(manufacture => new Manufacturer
                {
                    Code = manufacture.Field<string>("code") ?? "",
                    Name = manufacture.Field<string>("name") ?? string.Empty
                }).ToList();
        }

        public async Task UpdateManufacturersFromCloudApi(long startIndex, long limit, CancellationToken ct)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating manufacturers.", nameof(UpdateManufacturersFromCloudApi));

            await foreach (List<Manufacturer> manufacturers in GetManufacturersFromCloudApi(startIndex, limit))
            {
                await unitOfWork.Manufacturer.Upsert(manufacturers, ct);

                await unitOfWork.SaveAsync(ct);
            }

            _logger.LogInformation("[Method:{MethodName}] Manufacturers update completed.", nameof(UpdateManufacturersFromCloudApi));
        }

        public async Task UpdateManufacturersFromCloudDb(CancellationToken ct)
        {
            List<Manufacturer> manufacturers = await GetManufacturersFromCloudDb();

            await unitOfWork.Manufacturer.Upsert(manufacturers, ct);

            await unitOfWork.SaveAsync(ct);
        }
    }
}