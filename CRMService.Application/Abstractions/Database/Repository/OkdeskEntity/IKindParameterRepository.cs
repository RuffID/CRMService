using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IKindParameterRepository : IGetItemByIdRepository<KindsParameter, int>, IGetItemByPredicateRepository<KindsParameter>,  ICreateItemRepository<KindsParameter>
    {
    }
}


