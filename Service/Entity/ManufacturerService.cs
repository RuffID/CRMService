using CRMService.API;
using CRMService.DataBase.Postgresql;
using CRMService.Interfaces.Repository;
using CRMService.Models.ConfigClass;
using CRMService.Models.Entity;
using Microsoft.Extensions.Options;
using System.Data;

namespace CRMService.Service.Entity
{
    public class ManufacturerService(IOptions<ApiEndpoint> endpoint, IOptions<OkdeskSettings> okdeskSettings, GetItemService request, IUnitOfWorkEntities unitOfWork, PGSelect pGSelect, ILoggerFactory logger)
    {
        private readonly ILogger<ManufacturerService> _logger = logger.CreateLogger<ManufacturerService>();

        private async IAsyncEnumerable<List<Manufacturer>?> GetManufacturersFromCloudApi(long startIndex, long limit)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/manufacturers?api_token={okdeskSettings.Value.ApiToken}";

            await foreach (List<Manufacturer> manufacturers in request.GetAllItems<Manufacturer>(link, startIndex, limit))
                yield return manufacturers;
        }

        private async Task<List<Manufacturer>?> GetManufacturersFromCloudDb()
        {
            string sqlCommand = "SELECT * FROM equipment_manufacturers ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand);
            DataTable? table = ds.Tables["Table"];
            if (table == null)
                return null;

            return table.AsEnumerable().
                Select(manufacture => new Manufacturer
                {
                    Code = manufacture.Field<string>("code"),
                    Name = manufacture.Field<string>("name")
                }).ToList();
        }

        public async Task UpdateManufacturersFromCloudApi(long startIndex, long limit)
        {
            _logger.LogInformation("[Method:{MethodName}] Starting updating manufacturers.", nameof(UpdateManufacturersFromCloudApi));

            await foreach (List<Manufacturer>? manufacturers in GetManufacturersFromCloudApi(startIndex, limit))
            {
                if (manufacturers == null || manufacturers.Count == 0) return;

                await unitOfWork.Manufacturer.CreateOrUpdate(manufacturers);

                await unitOfWork.SaveAsync();
            }

            _logger.LogInformation("[Method:{MethodName}] Manufacturers update completed.", nameof(UpdateManufacturersFromCloudApi));
        }

        public async Task UpdateManufacturersFromCloudDb()
        {
            List<Manufacturer>? manufacturers = await GetManufacturersFromCloudDb();

            if (manufacturers == null || manufacturers.Count == 0)
                return;

            await unitOfWork.Manufacturer.CreateOrUpdate(manufacturers);

            await unitOfWork.SaveAsync();
        }
    }
}