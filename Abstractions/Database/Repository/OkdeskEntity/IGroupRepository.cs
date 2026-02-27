using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IGroupRepository : IGetItemByIdRepository<Group, int>, IGetItemByPredicateRepository<Group>, ICreateItemRepository<Group>
    {
    }
}