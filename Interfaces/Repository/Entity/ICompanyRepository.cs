using CRMService.Interfaces.Repository.Base;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface ICompanyRepository : IGetItemByIdRepository<Company, int>, IUpsertItemByIdRepository<Company, int>, ICreateItemRepository<Company>
    {
        Task<List<Company>> GetCompaniesByCategoryCode(string categoryCode, int startIndexCompany, int limit, CancellationToken ct);
    }
}
