using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IOkdeskRoleRepository : IGetItemByIdRepository<OkdeskRole, int>, IGetItemByPredicateRepository<OkdeskRole>,  ICreateItemRepository<OkdeskRole>
    {
    }
}