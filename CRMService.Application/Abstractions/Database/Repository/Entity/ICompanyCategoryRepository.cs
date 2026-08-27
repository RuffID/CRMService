using CRMService.Application.Abstractions.Database.Repository.Base;
using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Abstractions.Database.Repository.Entity
{
    public interface ICompanyCategoryRepository :
        IGetItemByIdRepository<CompanyCategory, int>,
        IGetItemsRepository<CompanyCategory>,
        ICreateItemRepository<CompanyCategory>
    {
        Task<CompanyCategory?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<CompanyCategory?> GetByCodeReadOnlyAsync(string code, CancellationToken ct = default);
    }
}
