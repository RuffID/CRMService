using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Service.OkdeskEntity.Resolvers
{
    public class KindResolverService(
        IUnitOfWork unitOfWork,
        KindService kindService,
        ReferenceResolveHelper referenceResolveHelper,
        ILogger<KindResolverService> logger)
    {
        public Task<int?> ResolveKindIdAsync(Kind kind, int equipmentId, CancellationToken ct)
        {
            return ResolveKindIdAsync(kind, "equipment", equipmentId, ct);
        }

        public Task<int?> ResolveKindIdAsync(Kind kind, string ownerEntityName, int ownerEntityId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(kind);

            return referenceResolveHelper.ResolveAsync(
                kind.Code,
                async token => (await unitOfWork.Kind.GetItemByPredicateAsync(k => k.Code == kind.Code, true, ct: token))?.Id,
                kindService.UpdateKindsFromCloudApi,
                code => $"kind:{code}",
                code => $"Kind with code: {code} was not found for {ownerEntityName} with id: {ownerEntityId}. Refreshing kinds from API.",
                code => $"Kind with code '{code}' was not found after refresh for {ownerEntityName} '{ownerEntityId}'.",
                logger,
                nameof(ResolveKindIdAsync),
                ct);
        }
    }
}