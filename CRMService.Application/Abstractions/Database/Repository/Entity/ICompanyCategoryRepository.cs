using Microsoft.EntityFrameworkCore;
using EFCoreLibrary.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface ICompanyCategoryRepository :
        IGetItemByIdRepository<CompanyCategory, int, DbContext>,
        IGetItemByPredicateRepository<CompanyCategory, DbContext>,
        ICreateItemRepository<CompanyCategory, DbContext>
    {
    }
}