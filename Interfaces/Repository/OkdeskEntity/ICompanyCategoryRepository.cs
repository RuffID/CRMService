using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface ICompanyCategoryRepository : 
        IGetItemByIdRepository<CompanyCategory, int>, 
        IGetItemByPredicateRepository<CompanyCategory>,
        ICreateItemRepository<CompanyCategory>, 
        IUpsertItemByIdRepository<CompanyCategory, int>
    {
    }
}