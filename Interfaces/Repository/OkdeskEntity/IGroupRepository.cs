using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IGroupRepository : IGetItemByIdRepository<Group, int>, IUpsertItemByIdRepository<Group, int>, ICreateItemRepository<Group>
    {
    }
}