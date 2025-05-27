using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeRepository : IGetRepository<Employee>, IUpdateRepository<Employee>, ICreateRepository<Employee>
    {
        Task<ICollection<Employee>?> GetEmployeesByGroup(int groupId, int startIndex, int limit);
        Task<Employee?> GetEmployeeById(int id, bool? trackable = null);
        Task<int> GetCountOfItems();
        Task CreateOrUpdate(IEnumerable<Employee> items);
    }
}