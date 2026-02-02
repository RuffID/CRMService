using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface ICompanyRepository : IGetItemByIdRepository<Company, int>, IGetItemByPredicateRepository<Company>, ICreateItemRepository<Company>
    {
    }
}