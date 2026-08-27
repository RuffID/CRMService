using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class ManufacturerResolverService(
        IEquipmentUnitOfWork unitOfWork,
        ManufacturerService manufacturerService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<ManufacturerResolverService> logger)
    {
        public Task<int?> ResolveManufacturerIdAsync(Manufacturer manufacturer, int equipmentId, CancellationToken ct)
        {
            return ResolveManufacturerIdAsync(manufacturer, "equipment", equipmentId, ct);
        }

        public Task<int?> ResolveManufacturerIdAsync(Manufacturer manufacturer, string ownerEntityName, int ownerEntityId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(manufacturer);

            return referenceResolveHelper.ResolveAsync(
                manufacturer.Code,
                async token => (await unitOfWork.Manufacturer.GetByCodeReadOnlyAsync(manufacturer.Code, token))?.Id,
                manufacturerService.UpdateManufacturersFromCloudApi,
                code => $"manufacturer:{code}",
                code => $"Manufacturer with code: {code} was not found for {ownerEntityName} with id: {ownerEntityId}. Refreshing manufacturers from API.",
                code => $"Manufacturer with code '{code}' was not found after refresh for {ownerEntityName} '{ownerEntityId}'.",
                logger,
                nameof(ResolveManufacturerIdAsync),
                ct);
        }
    }
}
