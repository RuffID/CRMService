using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IModelRepository : IGetItemByIdRepository<Model, int>, IGetItemByPredicateRepository<Model>, IUpsertItemByIdRepository<Model, int>, ICreateItemRepository<Model>
    {
    }
}