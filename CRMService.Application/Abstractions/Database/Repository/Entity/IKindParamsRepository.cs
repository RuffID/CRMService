using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IKindParamsRepository :
        ICreateItemRepository<KindParam>,
        IDeleteItemRepository<KindParam>
    {
        Task<List<KindParam>> GetByKindIdsReadOnlyAsync(IReadOnlyCollection<int> kindIds, CancellationToken ct = default);
    }
}
