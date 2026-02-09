using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface IKindParameterRepository : IGetItemByIdRepository<KindsParameter, int>, IGetItemByPredicateRepository<KindsParameter>,  ICreateItemRepository<KindsParameter>
    {
    }
}