using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface ICompanyRepository : IGetItemByIdRepository<Company, int>, IGetItemByPredicateRepository<Company>, ICreateItemRepository<Company>
    {
    }
}