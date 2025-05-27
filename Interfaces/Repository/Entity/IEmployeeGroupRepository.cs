using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeGroupRepository : IGetRepository<EmployeeGroup>, IUpdateRepository<EmployeeGroup>, ICreateRepository<EmployeeGroup>, IDeleteRepository<EmployeeGroup>
    {
        Task CreateOrUpdate(IEnumerable<EmployeeGroup> items);
        Task<EmployeeGroup?> GetConnectionByEmployeeAndGroup(int emoloyeeId, int groupId);
        Task<IEnumerable<EmployeeGroup>?> GetConnectionsByGroup(int groupId);
        Task<int> GetCountOfItems();
    }
}