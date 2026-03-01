using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface ICompanyCategoryRepository : IGetItemByIdRepository<CompanyCategory, int>, IGetItemByPredicateRepository<CompanyCategory>,
        ICreateItemRepository<CompanyCategory>
    {
    }
}


