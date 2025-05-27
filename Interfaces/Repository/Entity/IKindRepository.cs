using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindRepository : IGetRepository<Kind>, IUpdateRepository<Kind>, ICreateRepository<Kind>
    {
        Task<Kind?> GetKindByCode(string code, bool? trackable = null);
        Task CreateOrUpdate(IEnumerable<Kind> items);
        Task<int> GetCountOfItems();
    }
}