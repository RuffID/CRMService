using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class MaintenanceEntityResolverService(
        IUnitOfWork unitOfWork,
        MaintenanceEntityService maintenanceEntityService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<MaintenanceEntityResolverService> logger)
    {
        public Task<int?> ResolveMaintenanceEntityIdAsync(MaintenanceEntity maintenanceEntity, int issueId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(maintenanceEntity);

            return ResolveMaintenanceEntityIdAsync(maintenanceEntity.Id, issueId, ct);
        }

        public Task<int?> ResolveMaintenanceEntityIdAsync(int maintenanceEntityId, int issueId, CancellationToken ct)
        {
            return referenceResolveHelper.ResolveAsync(
                maintenanceEntityId,
                async token => (await unitOfWork.MaintenanceEntity.GetItemByIdAsync(maintenanceEntityId, true, ct: token))?.Id,
                token => maintenanceEntityService.UpdateMaintenanceEntityFromCloudApi(maintenanceEntityId, token),
                id => $"maintenance-entity:{id}",
                id => $"Service object with id: {id} was not found for issue with id: {issueId}. Refreshing maintenance entity from API.",
                id => $"Service object with id '{id}' was not found after refresh for issue '{issueId}'.",
                logger,
                nameof(ResolveMaintenanceEntityIdAsync),
                ct);
        }
    }
}