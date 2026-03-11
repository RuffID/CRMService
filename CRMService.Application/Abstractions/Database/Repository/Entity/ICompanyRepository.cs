using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface ICompanyRepository :
        IGetItemByIdRepository<Company, int, DbContext>,
        IGetItemByPredicateRepository<Company, DbContext>,
        ICreateItemRepository<Company, DbContext>
    {
    }
}