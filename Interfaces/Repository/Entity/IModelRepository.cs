using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IModelRepository : IGetItemByIdRepository<Model, int>, IGetItemByCodeRepository<Model>, IUpsertItemByCodeRepository<Model>, ICreateItemRepository<Model>
    {
    }
}