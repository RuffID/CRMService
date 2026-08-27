using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface ICrmRoleRepository :
        IGetItemByIdRepository<CrmRole, Guid>,
        IGetItemsRepository<CrmRole>,
        ICreateItemRepository<CrmRole>
    {
        Task<List<CrmRole>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    }
}
