using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IModelRepository : IGetItemByIdRepository<Model, int>, IGetItemByPredicateRepository<Model>, IUpsertItemByCodeRepository<Model>, ICreateItemRepository<Model>
    {
    }
}