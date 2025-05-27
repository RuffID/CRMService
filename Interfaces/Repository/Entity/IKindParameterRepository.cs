using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IKindParameterRepository : IGetRepository<KindsParameter>, IUpdateRepository<KindsParameter>, ICreateRepository<KindsParameter>
    {
        Task<KindsParameter?> GetKindParameterByCode(string code, bool? trackable = null);
        Task<int> GetCountOfItems();
        Task CreateOrUpdate(IEnumerable<KindsParameter> items);
    }
}