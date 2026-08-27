using CRMService.Application.Abstractions.Database.Repository;
using Microsoft.Extensions.Logging;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class KindParameterResolverService(
        IEquipmentUnitOfWork unitOfWork,
        KindParameterService kindParameterService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<KindParameterResolverService> logger)
    {
        public Task<int?> ResolveKindParameterIdAsync(string kindParameterCode, int equipmentId, CancellationToken ct)
        {
            return referenceResolveHelper.ResolveAsync(
                kindParameterCode,
                async token => (await unitOfWork.KindParameter.GetByCodeReadOnlyAsync(kindParameterCode, token))?.Id,
                kindParameterService.UpdateKindParametersFromCloudApi,
                code => $"kind-parameter:{code}",
                code => $"Kind parameter with code: {code} was not found for equipment with id: {equipmentId}. Refreshing kind parameters from API.",
                code => $"Kind parameter with code '{code}' was not found after refresh for equipment '{equipmentId}'.",
                logger,
                nameof(ResolveKindParameterIdAsync),
                ct);
        }
    }
}
