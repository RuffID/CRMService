using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class ModelResolverService(
        IEquipmentUnitOfWork unitOfWork,
        ModelService modelService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<ModelResolverService> logger)
    {
        public Task<int?> ResolveModelIdAsync(Model model, int equipmentId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(model);

            return referenceResolveHelper.ResolveAsync(
                model.Code,
                async token => (await unitOfWork.Model.GetByCodeReadOnlyAsync(model.Code, token))?.Id,
                modelService.UpdateModelsFromCloudApi,
                code => $"model:{code}",
                code => $"Model with code: {code} was not found for equipment with id: {equipmentId}. Refreshing models from API.",
                code => $"Model with code '{code}' was not found after refresh for equipment '{equipmentId}'.",
                logger,
                nameof(ResolveModelIdAsync),
                ct);
        }
    }
}
