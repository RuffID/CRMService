using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IKindRepository : IGetItemByIdRepository<Kind, int>, IGetItemByPredicateRepository<Kind>, ICreateItemRepository<Kind>
    {
    }
}


