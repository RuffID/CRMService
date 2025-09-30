using CRMService.Interfaces.Repository.Base;
using CRMService.Models.OkdeskEntity;

namespace CRMService.Interfaces.Repository.OkdeskEntity
{
    public interface ICompanyRepository : IGetItemByIdRepository<Company, int>, IUpsertItemByIdRepository<Company, int>, ICreateItemRepository<Company>
    {
        Task<List<Company>> GetCompaniesByCategoryCode(string categoryCode, int startIndexCompany, int limit, CancellationToken ct);
    }
}
