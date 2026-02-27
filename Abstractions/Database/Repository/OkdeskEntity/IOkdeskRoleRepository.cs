using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IOkdeskRoleRepository : IGetItemByIdRepository<OkdeskRole, int>, IGetItemByPredicateRepository<OkdeskRole>,  ICreateItemRepository<OkdeskRole>
    {
    }
}