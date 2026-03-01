using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IModelRepository : IGetItemByIdRepository<Model, int>, IGetItemByPredicateRepository<Model>, ICreateItemRepository<Model>
    {
    }
}


