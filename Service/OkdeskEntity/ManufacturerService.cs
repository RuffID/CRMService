using CRMService.Abstractions.Database.Repository;
using CRMService.DataBase.Postgresql;
using CRMService.Models.ConfigClass;
using CRMService.Models.Constants;
using CRMService.Models.OkdeskEntity;
using CRMService.Service.Requests;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime.CompilerServices;

namespace CRMService.Service.OkdeskEntity
{
    public class ManufacturerService(IOptions<ApiEndpointOptions> endpoint, IOptions<OkdeskOptions> okdeskSettings, GetOkdeskEntityService request, IUnitOfWork unitOfWork, PGSelect pGSelect, ILogger<ManufacturerService> logger)
    {
        private async IAsyncEnumerable<List<Manufacturer>> GetManufacturersFromCloudApi(long limit, [EnumeratorCancellation] CancellationToken ct)
        {
            string link = $"{endpoint.Value.OkdeskApi}/equipments/manufacturers?api_token={okdeskSettings.Value.OkdeskApiToken}";

            await foreach (List<Manufacturer> manufacturers in request.GetAllItems<Manufacturer>(link, startIndex: 0, limit, ct: ct))
                yield return manufacturers;
        }

        private async Task<List<Manufacturer>> GetManufacturersFromCloudDb(CancellationToken ct)
        {
            string sqlCommand = "SELECT * FROM equipment_manufacturers ORDER BY id;";

            DataSet ds = await pGSelect.Select(sqlCommand, ct);
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

        public async Task UpdateManufacturersFromCloudApi(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update manufacturers from API.", nameof(UpdateManufacturersFromCloudApi));

            await foreach (List<Manufacturer> manufacturers in GetManufacturersFromCloudApi(LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API, ct))
            {
                foreach (Manufacturer newManufacturer in manufacturers)
                {
                    Manufacturer? existingManufacturer = await unitOfWork.Manufacturer.GetItemByIdAsync(newManufacturer.Id, ct: ct);
                    if (existingManufacturer == null)
                        unitOfWork.Manufacturer.Create(newManufacturer);
                    else
                        existingManufacturer.CopyData(newManufacturer);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        public async Task UpdateManufacturersFromCloudDb(CancellationToken ct)
        {
            logger.LogInformation("[Method:{MethodName}] Starting to update manufacturers from DB.", nameof(UpdateManufacturersFromCloudDb));

            List<Manufacturer> manufacturers = await GetManufacturersFromCloudDb(ct);

            foreach (Manufacturer newManufacturer in manufacturers)
            {
                Manufacturer? existingManufacturer = await unitOfWork.Manufacturer.GetItemByIdAsync(newManufacturer.Id, ct: ct);
                if (existingManufacturer == null)
                    unitOfWork.Manufacturer.Create(newManufacturer);
                else
                    existingManufacturer.CopyData(newManufacturer);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}