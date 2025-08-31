using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface ICrmRoleRepository : IGetItemByIdRepository<CrmRole, Guid>, IGetItemByPredicateRepository<CrmRole>, ICreateItemRepository<CrmRole>
    {
        Task<List<CrmRole>> GetItemsByCollection(IEnumerable<CrmRole> items, bool asNoTracking = false, CancellationToken ct = default);
    }
}
