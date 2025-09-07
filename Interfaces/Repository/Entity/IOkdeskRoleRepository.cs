using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IOkdeskRoleRepository : IGetItemByIdRepository<OkdeskRole, int>, IGetItemByPredicateRepository<OkdeskRole>, IUpsertItemByIdRepository<OkdeskRole, int>, ICreateItemRepository<OkdeskRole>
    {
    }
}