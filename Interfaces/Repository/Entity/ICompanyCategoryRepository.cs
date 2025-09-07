using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface ICompanyCategoryRepository : 
        IGetItemByIdRepository<CompanyCategory, int>, 
        IGetItemByPredicateRepository<CompanyCategory>,
        ICreateItemRepository<CompanyCategory>, 
        IUpsertItemByIdRepository<CompanyCategory, int>,
        IUpsertItemByCodeRepository<CompanyCategory>
    {
    }
}