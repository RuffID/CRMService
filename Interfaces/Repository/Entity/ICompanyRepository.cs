using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface ICompanyRepository : IGetRepository<Company>, IUpdateRepository<Company>, ICreateRepository<Company>
    {
        Task<Company?> GetCompanyById(int id, bool? trackable = null);
        Task<IEnumerable<Company>?> GetCompaniesByCategoryCode(string categoryCode, int startIndexCompany, int limit);
        public Task CreateOrUpdate(IEnumerable<Company> items);
    }
}
