using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface IKindParameterRepository : IGetItemByIdRepository<KindsParameter, int>, IGetItemByPredicateRepository<KindsParameter>,  ICreateItemRepository<KindsParameter>
    {
    }
}