using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IModelRepository : IGetItemByIdRepository<Model, int>, IGetItemByPredicateRepository<Model>, ICreateItemRepository<Model>
    {
    }
}