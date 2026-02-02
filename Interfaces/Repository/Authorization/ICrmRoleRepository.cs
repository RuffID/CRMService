using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Repository.Authorization
{
    public interface ICrmRoleRepository : IGetItemByIdRepository<CrmRole, Guid>, IGetItemByPredicateRepository<CrmRole>, ICreateItemRepository<CrmRole>
    {
    }
}
