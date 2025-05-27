using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface ICompanyCategoryRepository : IGetRepository<CompanyCategory>, ICreateRepository<CompanyCategory>, IUpdateRepository<CompanyCategory>
    {
        public Task<int> GetCountOfItems();
        public Task CreateOrUpdate(IEnumerable<CompanyCategory> items);
    }
}
