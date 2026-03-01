using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Database.Repository.Authorization
{
    public interface ICrmRoleRepository : IGetItemByIdRepository<CrmRole, Guid>, IGetItemByPredicateRepository<CrmRole>, ICreateItemRepository<CrmRole>
    {
    }
}



