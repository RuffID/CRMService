using CRMService.Abstractions.Database.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Abstractions.Database.Repository.OkdeskEntity
{
    public interface ICompanyCategoryRepository : IGetItemByIdRepository<CompanyCategory, int>, IGetItemByPredicateRepository<CompanyCategory>,
        ICreateItemRepository<CompanyCategory>
    {
    }
}