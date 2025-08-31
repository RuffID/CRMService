using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IGroupRepository : IGetItemByIdRepository<Group, int>, IUpsertItemByIdRepository<Group, int>, ICreateItemRepository<Group>
    {
    }
}