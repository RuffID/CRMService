using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindParameterRepository : IGetItemByIdRepository<KindsParameter, int>, IGetItemByCodeRepository<KindsParameter>, IUpsertItemByCodeRepository<KindsParameter>, ICreateItemRepository<KindsParameter>
    {
    }
}