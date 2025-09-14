using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindParameterRepository : IGetItemByIdRepository<KindsParameter, int>, IGetItemByPredicateRepository<KindsParameter>, IUpsertItemByIdRepository<KindsParameter, int>, ICreateItemRepository<KindsParameter>
    {
    }
}