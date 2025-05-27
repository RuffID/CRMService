using CRMService.Interfaces.BaseRepository;
using CRMService.Models.Entity;

namespace CRMService.Interfaces.Repository.Entity
{
    public interface IEmployeeRoleRepository : IGetRepository<EmployeeRole>, IUpdateRepository<EmployeeRole>, ICreateRepository<EmployeeRole>, IDeleteRepository<EmployeeRole>
    {
        Task CreateOrUpdate(IEnumerable<EmployeeRole> items);
        Task<IEnumerable<EmployeeRole>?> GetConnectionsByEmployee(int employeeId);
    }
}