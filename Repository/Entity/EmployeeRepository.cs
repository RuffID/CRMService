using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class EmployeeRepository(CrmEntitiesContext context, ILoggerFactory logger) : IEmployeeRepository
    {
        private readonly ILogger<EmployeeRepository> _logger = logger.CreateLogger<EmployeeRepository>();

        public async Task<IEnumerable<Employee>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Employees.AsNoTracking().Where(c => c.Id >= startIndex).OrderBy(c => c.Id).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving employee list.", nameof(GetItems));
                return null;
            }
        }

        public async Task<Employee?> GetItem(Employee item, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Employees.FirstOrDefaultAsync(c => c.Id == item.Id);

                return await context.Employees.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving employee.", nameof(GetItem));
                return null;
            }
        }

        public async Task<Employee?> GetEmployeeById(int id, bool? trackable = null)
        {
            try
            {
                if (trackable == null || trackable == true)
                    return await context.Employees.FirstOrDefaultAsync(c => c.Id == id);

                return await context.Employees.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving employee by id.", nameof(GetEmployeeById));
                return null;
            }
        }

        public async Task<ICollection<Employee>?> GetEmployeesByGroup(int groupId, int startIndex, int limit)
        {
            try
            {
                return await context.EmployeeGroups
                    .AsNoTracking()
                    .Where(eg => eg.GroupId == groupId && eg.Employee.Id >= startIndex)
                    .OrderBy(eg => eg.Employee.Id)
                    .Take(limit)
                    .Select(eg => new Employee
                    {
                        Id = eg.Employee.Id,
                        FirstName = eg.Employee.FirstName,
                        LastName = eg.Employee.LastName,
                        Patronymic = eg.Employee.Patronymic,
                        Email = eg.Employee.Email,
                        Active = eg.Employee.Active,
                        Phone = eg.Employee.Phone,
                        Login = eg.Employee.Login,
                        Position = eg.Employee.Position
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving employees by group id.", nameof(GetEmployeesByGroup));
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.Employees.AsNoTracking().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error retrieving count of employees.", nameof(GetCountOfItems));
                return 0;
            }
        }

        public void Update(Employee oldItem, Employee newItem)
        {
            oldItem.CopyData(newItem);
        }

        public void Create(Employee item)
        {
            context.Employees.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Employee> items)
        {
            foreach (var item in items)
            {
                var existingItem = await GetItem(item);

                if (existingItem == null)
                    Create(item);
                else
                    Update(existingItem, item);
            }
        }
    }
}
