using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.OkdeskEntity
{
    public interface ICompanyRepository : IGetItemByIdRepository<Company, int>, IGetItemByPredicateRepository<Company>, ICreateItemRepository<Company>
    {
    }
}


