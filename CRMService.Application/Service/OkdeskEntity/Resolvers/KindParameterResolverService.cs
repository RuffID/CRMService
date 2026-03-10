using CRMService.Application.Abstractions.Database.Repository;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class KindParameterResolverService(
        IUnitOfWork unitOfWork,
        KindParameterService kindParameterService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<KindParameterResolverService> logger)
    {
        public Task<int?> ResolveKindParameterIdAsync(string kindParameterCode, int equipmentId, CancellationToken ct)
        {
            return referenceResolveHelper.ResolveAsync(
                kindParameterCode,
                async token => (await unitOfWork.KindParameter.GetItemByPredicateAsync(kp => kp.Code == kindParameterCode, true, ct: token))?.Id,
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