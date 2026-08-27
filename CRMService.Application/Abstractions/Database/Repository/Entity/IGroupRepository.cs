using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface IGroupRepository :
        IGetItemByIdRepository<Group, int>,
        IGetItemsRepository<Group>,
        ICreateItemRepository<Group>
    {
        Task<List<Group>> SearchReadOnlyAsync(string? search, CancellationToken ct = default);
        Task<List<Group>> GetByIdsReadOnlyAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    }
}
