using Microsoft.EntityFrameworkCore;
using CRMService.DataBase;
using CRMService.Models.Entity;
using CRMService.Interfaces.Repository.Entity;

namespace CRMService.Repository.Entity
{
    public class EmployeeRepository(CRMEntitiesContext context, ILoggerFactory logger) : IEmployeeRepository
    {
        private readonly ILogger<EmployeeRepository> _logger = logger.CreateLogger<EmployeeRepository>();

        public async Task<IEnumerable<Employee>?> GetItems(int startIndex, int limit)
        {
            try
            {
                return await context.Employees.AsNoTracking().OrderBy(c => c.Id).Where(c => c.Id >= startIndex).Take(limit).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee list.");
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
                _logger.LogError(ex, "Error retrieving employee.");
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
                _logger.LogError(ex, "Error retrieving employee by id.");
                return null;
            }
        }

        public async Task<ICollection<Employee>?> GetEmployeesByGroup(int groupId, int startIndex, int limit)
        {
            try
            {
                return await (from employee in context.Employees
                              join employeeGroups in context.EmployeeGroups on employee.Id equals employeeGroups.EmployeeId
                              where employeeGroups.GroupId == groupId && employee.Id >= startIndex
                              orderby employee.Id
                              select new Employee()
                              {
                                  Id = employee.Id,
                                  FirstName = employee.FirstName,
                                  LastName = employee.LastName,
                                  Patronymic = employee.Patronymic,
                                  Email = employee.Email,
                                  Active = employee.Active,
                                  Phone = employee.Phone,
                                  Login = employee.Login,
                                  Position = employee.Position
                              })
                              .Take(limit)
                              .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees by group id.");
                return null;
            }
        }

        public async Task<int> GetCountOfItems()
        {
            try
            {
                return await context.Employees.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving count of employees.");
                return 0;
            }
        }

        public void Update(Employee item)
        {
            context.Entry(item).State = EntityState.Modified;
        }

        public void Create(Employee item)
        {
            context.Employees.Add(item);
        }

        public async Task CreateOrUpdate(IEnumerable<Employee> items)
        {
            foreach (var item in items)
            {
                if (await GetItem(item, false) == null)
                    Create(item);
                else
                    Update(item);
            }
        }
    }
}
