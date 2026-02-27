using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IKindRepository : IGetItemByIdRepository<Kind, int>, IGetItemByPredicateRepository<Kind>, ICreateItemRepository<Kind>
    {
    }
}