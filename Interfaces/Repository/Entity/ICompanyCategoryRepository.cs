using CRMService.Interfaces.Repository.Base;
using CRMService.Interfaces.Repository.Extended;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface ICompanyCategoryRepository : 
        IGetItemByIdRepository<CompanyCategory, int>, 
        IGetItemByCodeRepository<CompanyCategory>,
        ICreateItemRepository<CompanyCategory>, 
        IUpsertItemByIdRepository<CompanyCategory, int>,
        IUpsertItemByCodeRepository<CompanyCategory>,
        ICountItemRepository<CompanyCategory>
    {
    }
}