using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.Authorization;

namespace CRMService.Abstractions.Database.Repository.Authorization
{
    public interface ICrmRoleRepository : IGetItemByIdRepository<CrmRole, Guid>, IGetItemByPredicateRepository<CrmRole>, ICreateItemRepository<CrmRole>
    {
    }
}
